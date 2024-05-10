using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace PriorityQueues.Consume;

public class PriorityConsumer(IModel channel, ConsumerFactory consumerFactory, SemaphoreSlim messagesToProcessSemaphore, string queueName)
{
    private readonly IModel _channel = channel;
    private readonly ConcurrentQueue<(ulong deliveryTag, byte[] body)> _deliveries = new();
    private readonly ConsumerFactory _consumerFactory = consumerFactory;
    private readonly SemaphoreSlim _deliveriesToProcessSemaphore = messagesToProcessSemaphore;
    private readonly string _queueName = queueName;

    public void Start()
    {
        var consumer = _consumerFactory.GetConsumer(_channel);
        consumer.Received += (ch, ea) =>
        {
            _deliveries.Enqueue((ea.DeliveryTag, ea.Body.ToArray()));
            _deliveriesToProcessSemaphore.Release();
        };
        _channel.BasicConsume(_queueName, autoAck: false, consumer);
    }

    public ConcurrentQueue<(ulong deliveryTag, byte[] body)> GetDeliveries()
    {
        return _deliveries;
    }
}
