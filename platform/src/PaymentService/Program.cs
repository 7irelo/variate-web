using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Variate.BuildingBlocks.Contracts;
using Variate.PaymentService;
using Variate.PaymentService.Gateways;
using Variate.PaymentService.Messaging;
using Variate.PaymentService.Models;

var builder = WebApplication.CreateBuilder(args);

var paymentConnection = builder.Configuration.GetConnectionString("PaymentDb")
    ?? "Server=sqlserver,1433;Database=variate_payment;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;";
builder.Services.AddDbContext<PaymentDbContext>(options => options.UseSqlServer(paymentConnection));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
        };
    });
builder.Services.AddAuthorization();

var rabbitOptions = builder.Configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions();
builder.Services.AddSingleton(rabbitOptions);
builder.Services.AddSingleton<RabbitMqPublisher>();

var stripeOptions = builder.Configuration.GetSection(StripeGatewayOptions.SectionName).Get<StripeGatewayOptions>() ?? new StripeGatewayOptions();
builder.Services.AddSingleton(stripeOptions);

builder.Services.AddSingleton<IPaymentGateway, FakeGateway>();
builder.Services.AddSingleton<IPaymentGateway>(sp => new StripeGateway(sp.GetRequiredService<StripeGatewayOptions>()));
builder.Services.AddScoped<PaymentProcessor>();
builder.Services.AddHostedService<PaymentRequestedConsumer>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await db.Database.EnsureCreatedAsync();
}

var payments = app.MapGroup("/api/v1/payments");

payments.MapPost("/checkout", async (PaymentCheckoutRequest request, PaymentProcessor processor, CancellationToken cancellationToken) =>
{
    var result = await processor.ProcessAsync(new ProcessPaymentRequest(
        request.PaymentId ?? Guid.NewGuid(),
        request.OrderId,
        request.UserId.Trim(),
        request.Amount,
        request.Currency,
        request.Provider,
        request.PaymentMethodToken), cancellationToken);

    return Results.Ok(ToResponse(result));
}).RequireAuthorization();

payments.MapGet("/{id:guid}", async (PaymentDbContext db, Guid id) =>
{
    var payment = await db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    return payment is null ? Results.NotFound() : Results.Ok(ToResponse(payment));
}).RequireAuthorization();

payments.MapGet("/order/{orderId:guid}", async (PaymentDbContext db, Guid orderId) =>
{
    var payment = await db.Payments.AsNoTracking()
        .Where(p => p.OrderId == orderId)
        .OrderByDescending(p => p.CreatedAtUtc)
        .FirstOrDefaultAsync();
    return payment is null ? Results.NotFound() : Results.Ok(ToResponse(payment));
}).RequireAuthorization();

payments.MapPost("/webhook/stripe", async (HttpRequest request, ILoggerFactory loggerFactory) =>
{
    using var reader = new StreamReader(request.Body);
    var payload = await reader.ReadToEndAsync();
    var logger = loggerFactory.CreateLogger("StripeWebhook");
    logger.LogInformation("Received Stripe webhook payload: {Payload}", payload);
    return Results.Accepted();
});

payments.MapGet("/health", () => Results.Ok(new { service = "payment-service", status = "ok" }));

app.Run();

static PaymentResponse ToResponse(PaymentEntity payment)
{
    return new PaymentResponse(
        payment.Id,
        payment.OrderId,
        payment.UserId,
        payment.Amount,
        payment.Currency,
        payment.Provider,
        payment.Status,
        payment.ExternalReference,
        payment.FailureReason,
        payment.CreatedAtUtc,
        payment.UpdatedAtUtc);
}

internal sealed class PaymentProcessor(
    PaymentDbContext db,
    IEnumerable<IPaymentGateway> gateways,
    RabbitMqPublisher publisher,
    ILogger<PaymentProcessor> logger)
{
    private readonly PaymentDbContext _db = db;
    private readonly IEnumerable<IPaymentGateway> _gateways = gateways;
    private readonly RabbitMqPublisher _publisher = publisher;
    private readonly ILogger<PaymentProcessor> _logger = logger;

    public async Task<PaymentEntity> ProcessAsync(ProcessPaymentRequest request, CancellationToken cancellationToken)
    {
        var existing = await _db.Payments.FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var payment = new PaymentEntity
        {
            Id = request.PaymentId,
            OrderId = request.OrderId,
            UserId = request.UserId,
            Amount = request.Amount,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            Provider = request.Provider.Trim().ToLowerInvariant(),
            Status = "Pending",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(cancellationToken);

        var gateway = ResolveGateway(payment.Provider);
        var outcome = await gateway.ChargeAsync(new GatewayChargeRequest(
            payment.Id,
            payment.OrderId,
            payment.UserId,
            payment.Amount,
            payment.Currency,
            request.PaymentMethodToken), cancellationToken);

        payment.Status = outcome.Status;
        payment.ExternalReference = outcome.ExternalReference;
        payment.FailureReason = outcome.FailureReason;
        payment.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var completedEvent = new PaymentCompletedEvent(
            payment.Id,
            payment.OrderId,
            payment.UserId,
            payment.Amount,
            payment.Currency,
            payment.Provider,
            payment.Status,
            payment.ExternalReference,
            DateTime.UtcNow);

        _publisher.Publish("payment.completed", completedEvent);
        _logger.LogInformation("Processed payment {PaymentId} for order {OrderId} with status {Status}.",
            payment.Id, payment.OrderId, payment.Status);

        return payment;
    }

    private IPaymentGateway ResolveGateway(string provider)
    {
        return _gateways.FirstOrDefault(g => g.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase))
            ?? _gateways.First(g => g.Provider == "fake");
    }
}

internal sealed class PaymentRequestedConsumer(IServiceProvider serviceProvider, RabbitMqOptions options, ILogger<PaymentRequestedConsumer> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly RabbitMqOptions _options = options;
    private readonly ILogger<PaymentRequestedConsumer> _logger = logger;
    private IConnection? _connection;
    private IModel? _channel;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false);

        const string queueName = "payment-service-payment-requested";
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queueName, _options.Exchange, "payment.requested");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += HandleMessageAsync;
        _channel.BasicConsume(queueName, autoAck: false, consumer);
        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        try
        {
            if (_channel is null)
            {
                return;
            }

            var json = Encoding.UTF8.GetString(args.Body.ToArray());
            var message = JsonSerializer.Deserialize<PaymentRequestedEvent>(json);
            if (message is null)
            {
                _channel.BasicAck(args.DeliveryTag, false);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<PaymentProcessor>();
            await processor.ProcessAsync(new ProcessPaymentRequest(
                message.PaymentId,
                message.OrderId,
                message.UserId,
                message.Amount,
                message.Currency,
                message.Provider,
                null), CancellationToken.None);

            _channel.BasicAck(args.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment request event.");
            _channel?.BasicNack(args.DeliveryTag, false, true);
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}

internal sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = "variate.auth";
    public string Audience { get; init; } = "variate.services";
    public string SigningKey { get; init; } = "replace-this-in-real-deployments-with-a-long-secret";
}

internal sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public string HostName { get; init; } = "rabbitmq";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string Exchange { get; init; } = "variate.events";
}

internal sealed record ProcessPaymentRequest(
    Guid PaymentId,
    Guid OrderId,
    string UserId,
    decimal Amount,
    string Currency,
    string Provider,
    string? PaymentMethodToken);

internal sealed record PaymentCheckoutRequest(
    Guid? PaymentId,
    Guid OrderId,
    string UserId,
    decimal Amount,
    string Currency,
    string Provider,
    string? PaymentMethodToken);

internal sealed record PaymentResponse(
    Guid PaymentId,
    Guid OrderId,
    string UserId,
    decimal Amount,
    string Currency,
    string Provider,
    string Status,
    string? ExternalReference,
    string? FailureReason,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
