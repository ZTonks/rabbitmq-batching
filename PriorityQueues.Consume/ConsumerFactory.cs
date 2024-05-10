using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PriorityQueues.Consume;

public class ConsumerFactory
{
    public EventingBasicConsumer GetConsumer(IModel channel)
    {
        return new EventingBasicConsumer(channel);
    }
}
