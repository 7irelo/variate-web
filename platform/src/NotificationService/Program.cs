using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddHostedService<NotificationConsumerWorker>();

var host = builder.Build();
host.Run();

internal sealed class NotificationConsumerWorker(
    ILogger<NotificationConsumerWorker> logger,
    IOptions<RabbitMqOptions> rabbitOptions) : BackgroundService
{
    private readonly ILogger<NotificationConsumerWorker> _logger = logger;
    private readonly RabbitMqOptions _options = rabbitOptions.Value;
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

        const string queueName = "notification-service-events";
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queueName, _options.Exchange, "order.created");
        _channel.QueueBind(queueName, _options.Exchange, "order.cancelled");
        _channel.QueueBind(queueName, _options.Exchange, "payment.completed");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnMessageAsync;
        _channel.BasicConsume(queueName, autoAck: false, consumer);

        _logger.LogInformation("Notification worker is consuming domain events.");
        return Task.CompletedTask;
    }

    private Task OnMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        try
        {
            if (_channel is null)
            {
                return Task.CompletedTask;
            }

            var body = Encoding.UTF8.GetString(args.Body.ToArray());
            _logger.LogInformation("Event received. RoutingKey: {RoutingKey} Payload: {Payload}", args.RoutingKey, body);
            _channel.BasicAck(args.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process notification event.");
            _channel?.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
        }

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
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
