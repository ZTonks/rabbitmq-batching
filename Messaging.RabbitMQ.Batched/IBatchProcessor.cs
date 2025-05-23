namespace Messaging.RabbitMQ.Batched;

public interface IBatchProcessor
{
    public Task ProcessMessages(
        ICollection<(ulong deliveryTag, byte[] body)> messages,
        CancellationToken cancellationToken);
}
