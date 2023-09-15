namespace TP.Rabbit;

public interface IRabbitMqProducer
{
    bool Publish<TMessage>(TMessage message, string routingKey);
}