namespace Messaging.RabbitMQ.Batched.Options;

public class RabbitMQOptions
{
    public ushort Port { get; init; } = 5672;

    public string SubscriptionId { get; init; } = "default_subscription_id";

    public required string ConsumerUserName { get; init; }

    public required string ConsumerPassword { get; init; }

    public required string ConsumerVirtualHost { get; init; }

    public required string ConsumerHostName { get; init; }
}
