using RabbitMQ.Client;
using System.Text;

namespace PriorityQueues.Produce;

public class PriorityProducer(IModel channel, string queueName)
{
    private readonly IModel _channel = channel;
    private readonly string _queueName = queueName;

    public void ProduceMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: string.Empty, routingKey: _queueName, body: body);
    }
}
