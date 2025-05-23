using RabbitMQ.Client;
using System.Text;

namespace PriorityQueues.Produce;

public class MessageProducerHostedService : IHostedService
{
    private const int _delay = 1;
    private const string _queueName = "batch-test";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
        channel.QueueDeclare(_queueName, exclusive: false, autoDelete: false);
        var i = 0;

        do
        {
            var msg = new
            {
                Index = i++,
            };

            var bytes = Encoding.UTF8.GetBytes(msg.ToString()!);

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: _queueName,
                basicProperties: null,
                body: bytes);

            await Task.Delay(_delay, cancellationToken);
        }
        while (true);
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
