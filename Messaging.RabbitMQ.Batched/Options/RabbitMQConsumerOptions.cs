namespace Messaging.RabbitMQ.Batched.Options;

public class RabbitMQConsumerOptions
{
    public ushort PrefetchCount { get; set; } = 100;

    public ushort MinMessageBatchSize { get; set; } = 1;

    public ushort MaxMessageBatchSize { get; set; } = 100;

    public ushort MaxBatchWaitTimeMs { get; set; } = 100;

    public ushort BufferTimeMs { get; set; } = 100;

    public ushort? MaxRetries { get; set; } = 5;

    /// <summary>
    /// null if no retry strategy.
    /// false if static retry strategy.
    /// true if exponential retry
    /// </summary>
    public bool? IsExponentialRetryStrategy { get; set; } = false;

    public required string QueueName { get; set; }
}
 