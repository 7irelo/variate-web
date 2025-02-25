using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Variate.PaymentService.Messaging;

internal sealed class RabbitMqPublisher(RabbitMqOptions options)
{
    private readonly RabbitMqOptions _options = options;

    public void Publish<T>(string routingKey, T payload)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(_options.Exchange, routingKey, properties, body);
    }
}
