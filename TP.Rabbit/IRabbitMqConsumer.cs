namespace TP.Rabbit;

public interface IRabbitMqConsumer
{
    public interface IQueueConsumer
    {
        void StartConsuming<TMessage>(
            string queueName,
            ushort prefetchCount,
            Func<TMessage, CancellationToken, Task<bool>> messageHandler,
            CancellationToken ct,
            ushort? maxRetry = null);
    }
}