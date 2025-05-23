using Messaging.RabbitMQ.Batched.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;

namespace Messaging.RabbitMQ.Batched.Misc;

internal class BatchedConsumer(
    IModel channel,
    IOptions<RabbitMQConsumerOptions> options)
{
    private readonly ConcurrentQueue<(ulong deliveryTag, byte[] body)> _deliveries = new();
    private readonly SemaphoreSlim _deliveriesSemaphore = new(options.Value.MaxMessageBatchSize);

    internal void Start()
    {
        var queueName = options.Value.QueueName;

        channel.QueueDeclare(queueName, exclusive: false, autoDelete: false);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (ch, ea) =>
        {
            _deliveriesSemaphore.Wait();
            _deliveries.Enqueue((ea.DeliveryTag, ea.Body.ToArray()));
        };

        channel.BasicConsume(queueName, autoAck: false, consumer);
    }

    internal ConcurrentQueue<(ulong deliveryTag, byte[] body)> Deliveries => _deliveries;

    internal SemaphoreSlim Semaphore => _deliveriesSemaphore;
}
