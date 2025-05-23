using Messaging.RabbitMQ.Batched.Options;

namespace Messaging.RabbitMQ.Batched.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection ConfigureBatchedMessageHandler<T>(
        this IServiceCollection services,
        bool configureDefaultBatchProcessor = false)
            where T : class, IBatchProcessor
    {
        services
            .AddOptions<RabbitMQOptions>()
            .BindConfiguration("RabbitMQ");
        
        services
            .AddOptions<RabbitMQConsumerOptions>()
            .BindConfiguration(nameof(RabbitMQConsumerOptions));

        if (configureDefaultBatchProcessor)
        {
            services.AddSingleton<T>();
        }

        return services
            .AddHostedService<MessageHandlerHostedService<T>>();
    }
}
