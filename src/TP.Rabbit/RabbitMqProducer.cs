using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace TP.Rabbit;

public class RabbitMqProducer : IRabbitMqProducer
{
    private readonly RabbitMqConnection _rabbitConnection;
    private readonly ILogger<RabbitMqProducer> _logger;
    private readonly IModel _channel;

    public RabbitMqProducer(RabbitMqConnection rabbitConnection, ILogger<RabbitMqProducer> logger)
    {
        _rabbitConnection = rabbitConnection;
        _logger = logger;
        _channel = Connection.CreateModel();
    }

    private IConnection Connection => _rabbitConnection.Connection;

    public bool Publish<TMessage>(TMessage message, string routingKey)
    {
        var serializedMessage = JsonSerializer.Serialize(message);

        return PublishMessage(serializedMessage, routingKey);
    }

    private bool PublishMessage(string serializedMessage, string routingKey, string exchange = "")
    {
        try
        {
            var byteArray = Encoding.UTF8.GetBytes(serializedMessage);

            var properties = _channel.CreateBasicProperties();

            properties.Persistent = true;
            properties.CreateRetryCountHeader();

            _channel.BasicPublish(
                exchange,
                routingKey,
                properties,
                byteArray);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception captured on {Method} - {@DataPublished}",
                nameof(PublishMessage), new { serializedMessage, routingKey });

            return false;
        }
    }
}
