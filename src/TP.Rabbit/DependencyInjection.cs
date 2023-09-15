using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace TP.Rabbit;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<IServiceProvider, IConnectionFactory>? configureConnectionFactory = null)
    {
        var configurationSection = configuration.GetSection(nameof(RabbitMqSettings))!;

        var rabbitSettings = configurationSection.Get<RabbitMqSettings>()!;

        services.Configure<RabbitMqSettings>(configurationSection);

        services.AddSingleton<IConnectionFactory>(x =>
            configureConnectionFactory?.Invoke(x)
                ?? DefaultConnectionFactory(rabbitSettings));

        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IRabbitMqConsumer, RabbitMqConsumer>();
        services.AddScoped<IRabbitMqProducer, RabbitMqProducer>();

        return services;
    }

    private static ConnectionFactory DefaultConnectionFactory(RabbitMqSettings rabbitSettings)
    {
        return new ConnectionFactory
        {
            HostName = rabbitSettings.HostName,
            Port = rabbitSettings.Port,
            UserName = rabbitSettings.UserName,
            Password = rabbitSettings.Password,
            DispatchConsumersAsync = true,
            ConsumerDispatchConcurrency = 1,
            UseBackgroundThreadsForIO = false
        };
    }
}
