using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace TP.Rabbit;

public class RabbitMqConnection
{
    private readonly IConnectionFactory _factory;

    private static readonly object ConnectionLocker = new();

    private static readonly IEnumerable<TimeSpan> SleepsBetweenRetries = new List<TimeSpan>
    {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(3),
    };

    public RabbitMqConnection(IConnectionFactory factory)
    {
        _factory = factory;
    }

    private IConnection? _connection;
    public IConnection Connection => GetConnection();

    private IConnection GetConnection()
    {
        if (_connection is { IsOpen: true }) return _connection;
        lock (ConnectionLocker)
        {
            return _connection is { IsOpen: true }
                ? _connection
                : RetryConnect();
        }
    }

    private IConnection RetryConnect()
    {
        _connection?.Dispose();
        var retryAttempt = 0;
        while (true)
        {
            try
            {
                _connection = _factory.CreateConnection();
                return _connection;
            }
            catch (BrokerUnreachableException)
            {
                retryAttempt++;

                if (retryAttempt >= SleepsBetweenRetries.Count())
                {
                    throw;
                }

                Thread.Sleep(SleepsBetweenRetries.ElementAt(retryAttempt));
            }
        }
    }
}