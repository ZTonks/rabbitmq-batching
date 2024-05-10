using RabbitMQ.Client;

namespace PriorityQueues.Produce;

public class MessageProducerHostedService : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        for (var priority = 0; priority < 5; priority++)
        {
            var queueName = $"priority-queue-{priority}";
            channel.QueueDeclare(queueName, exclusive: false, autoDelete: false);
            var producer = new PriorityProducer(channel, queueName);

            for (var i = 0; i < 3000; i++)
            {
                producer.ProduceMessage($"priority {priority} - message {i}");
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
