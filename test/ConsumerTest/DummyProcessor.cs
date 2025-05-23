using Messaging.RabbitMQ.Batched;

namespace ConsumerTest;

internal class DummyProcessor : IBatchProcessor
{
    public Task ProcessMessages(
        ICollection<(ulong deliveryTag, byte[] body)> messages,
        CancellationToken cancellationToken)
    {
        foreach (var item in messages)
        {
            Console.WriteLine(item);
        }

        return Task.CompletedTask;
    }
}
