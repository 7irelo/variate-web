using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Variate.BuildingBlocks.Contracts;
using Variate.OrderService;
using Variate.OrderService.Messaging;
using Variate.OrderService.Models;

var builder = WebApplication.CreateBuilder(args);

var orderConnection = builder.Configuration.GetConnectionString("OrderDb")
    ?? "Server=sqlserver,1433;Database=variate_order;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;";

builder.Services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(orderConnection));

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
builder.Services.AddHostedService<PaymentCompletedConsumer>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await db.Database.EnsureCreatedAsync();
}

var order = app.MapGroup("/api/v1/orders");

order.MapPost("/checkout", async (
    CheckoutRequest request,
    OrderDbContext db,
    RabbitMqPublisher publisher) =>
{
    if (request.Items.Count == 0)
    {
        return Results.BadRequest(new { message = "At least one line item is required." });
    }

    var subtotal = request.Items.Sum(i => i.UnitPrice * i.Quantity);
    var discountRate = CouponEngine.ResolveDiscountRate(request.CouponCode);
    var discountAmount = Math.Round(subtotal * discountRate, 2);
    var total = subtotal - discountAmount;
    var currency = string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency.Trim().ToUpperInvariant();

    var orderEntity = new OrderEntity
    {
        UserId = request.UserId.Trim(),
        Status = "PendingPayment",
        Currency = currency,
        Subtotal = subtotal,
        DiscountAmount = discountAmount,
        Total = total,
        DiscountCode = request.CouponCode?.Trim().ToUpperInvariant(),
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow,
        Items = request.Items.Select(item => new OrderItemEntity
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName.Trim(),
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        }).ToList()
    };

    db.Orders.Add(orderEntity);
    await db.SaveChangesAsync();

    var orderCreated = new OrderCreatedEvent(
        orderEntity.Id,
        orderEntity.UserId,
        orderEntity.Subtotal,
        orderEntity.DiscountAmount,
        orderEntity.Total,
        orderEntity.Currency,
        DateTime.UtcNow,
        orderEntity.Items.Select(i => new OrderLine(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList());

    publisher.Publish("order.created", orderCreated);

    var paymentId = Guid.NewGuid();
    var paymentRequested = new PaymentRequestedEvent(
        paymentId,
        orderEntity.Id,
        orderEntity.UserId,
        orderEntity.Total,
        orderEntity.Currency,
        request.PaymentProvider.Trim().ToLowerInvariant(),
        DateTime.UtcNow);

    publisher.Publish("payment.requested", paymentRequested);

    return Results.Accepted($"/api/v1/orders/{orderEntity.Id}", new
    {
        orderId = orderEntity.Id,
        paymentId,
        status = orderEntity.Status,
        orderEntity.Subtotal,
        orderEntity.DiscountAmount,
        orderEntity.Total,
        orderEntity.Currency
    });
}).RequireAuthorization();

order.MapGet("/{id:guid}", async (OrderDbContext db, Guid id) =>
{
    var entity = await db.Orders
        .AsNoTracking()
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == id);

    return entity is null ? Results.NotFound() : Results.Ok(ToResponse(entity));
}).RequireAuthorization();

order.MapGet("/user/{userId}", async (OrderDbContext db, string userId) =>
{
    var entities = await db.Orders
        .AsNoTracking()
        .Where(o => o.UserId == userId)
        .Include(o => o.Items)
        .OrderByDescending(o => o.CreatedAtUtc)
        .ToListAsync();

    var orders = entities.Select(ToResponse).ToList();
    return Results.Ok(orders);
}).RequireAuthorization();

order.MapPatch("/{id:guid}/status", async (OrderDbContext db, Guid id, UpdateStatusRequest request) =>
{
    var entity = await db.Orders.FirstOrDefaultAsync(o => o.Id == id);
    if (entity is null)
    {
        return Results.NotFound();
    }

    entity.Status = request.Status.Trim();
    entity.UpdatedAtUtc = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(new { entity.Id, entity.Status });
}).RequireAuthorization();

order.MapPost("/{id:guid}/cancel", async (OrderDbContext db, Guid id, RabbitMqPublisher publisher) =>
{
    var entity = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
    if (entity is null)
    {
        return Results.NotFound();
    }

    if (entity.Status is "Paid" or "Shipped" or "Delivered")
    {
        return Results.BadRequest(new { message = "Order cannot be cancelled in its current state." });
    }

    entity.Status = "Cancelled";
    entity.UpdatedAtUtc = DateTime.UtcNow;
    await db.SaveChangesAsync();

    publisher.Publish("order.cancelled", new
    {
        orderId = entity.Id,
        entity.UserId,
        entity.Total,
        entity.Currency,
        cancelledAtUtc = DateTime.UtcNow
    });

    return Results.Ok(new { entity.Id, entity.Status });
}).RequireAuthorization();

order.MapGet("/health", () => Results.Ok(new { service = "order-service", status = "ok" }));

app.Run();

static OrderResponse ToResponse(OrderEntity entity)
{
    return new OrderResponse(
        entity.Id,
        entity.UserId,
        entity.Status,
        entity.Subtotal,
        entity.DiscountAmount,
        entity.Total,
        entity.Currency,
        entity.DiscountCode,
        entity.CreatedAtUtc,
        entity.UpdatedAtUtc,
        entity.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList());
}

internal sealed class PaymentCompletedConsumer(IServiceProvider serviceProvider, RabbitMqOptions options, ILogger<PaymentCompletedConsumer> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly RabbitMqOptions _options = options;
    private readonly ILogger<PaymentCompletedConsumer> _logger = logger;
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

        const string queueName = "order-service-payment-completed";
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queueName, _options.Exchange, "payment.completed");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnMessageAsync;
        _channel.BasicConsume(queueName, autoAck: false, consumer);
        return Task.CompletedTask;
    }

    private async Task OnMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        try
        {
            if (_channel is null)
            {
                return;
            }

            var payload = Encoding.UTF8.GetString(args.Body.ToArray());
            var message = JsonSerializer.Deserialize<PaymentCompletedEvent>(payload);
            if (message is null)
            {
                _channel.BasicAck(args.DeliveryTag, multiple: false);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == message.OrderId);
            if (order is not null)
            {
                order.Status = message.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                    ? "Paid"
                    : "PaymentFailed";
                order.UpdatedAtUtc = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }

            _channel.BasicAck(args.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment completion message.");
            _channel?.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}

internal static class CouponEngine
{
    private static readonly Dictionary<string, decimal> CouponRates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["WELCOME10"] = 0.10m,
        ["VIP20"] = 0.20m,
        ["FREESHIP"] = 0.05m
    };

    public static decimal ResolveDiscountRate(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return 0m;
        }

        return CouponRates.TryGetValue(code.Trim(), out var rate) ? rate : 0m;
    }
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

internal sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = "variate.auth";
    public string Audience { get; init; } = "variate.services";
    public string SigningKey { get; init; } = "replace-this-in-real-deployments-with-a-long-secret";
}

internal sealed record CheckoutRequest(
    string UserId,
    string Currency,
    string PaymentProvider,
    string? CouponCode,
    IReadOnlyCollection<CheckoutItemRequest> Items);

internal sealed record CheckoutItemRequest(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);
internal sealed record UpdateStatusRequest(string Status);
internal sealed record OrderItemResponse(Guid Id, Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);
internal sealed record OrderResponse(
    Guid Id,
    string UserId,
    string Status,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    string Currency,
    string? DiscountCode,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<OrderItemResponse> Items);
