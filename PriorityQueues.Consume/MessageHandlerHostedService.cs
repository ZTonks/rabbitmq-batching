using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace PriorityQueues.Consume;

public class MessageHandlerHostedService(ConsumerFactory consumerFactory, IOptions<QueueOptions> queueOptions) : BackgroundService
{
    private readonly ConsumerFactory _consumerFactory = consumerFactory;
    private readonly QueueOptions queueOptions = queueOptions.Value;
    private readonly SemaphoreSlim _deliveriesToProcessSemaphore = new(0);
    private readonly int _messageWeight = 1;

    private IConnection? _connection;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            var channel = SetUpChannel();
            var slots = SetUpPrioritySlots(_deliveriesToProcessSemaphore, channel);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_deliveriesToProcessSemaphore.CurrentCount > 0)
                {
                    foreach (var slot in slots)
                    {
                        slot.IncreaseDeficit();

                        while (slot.Deficit >= _messageWeight && slot.Consumer.GetDeliveries().TryDequeue(out var delivery))
                        {
                            Task.Factory.StartNew(() =>
                            {
                                var body = delivery.body;
                                var message = System.Text.Encoding.UTF8.GetString(body, 0, body.Length);

                                Console.WriteLine(message);

                                channel.BasicAck(delivery.deliveryTag, multiple: false);
                                _deliveriesToProcessSemaphore.Wait();
                            }, TaskCreationOptions.LongRunning);

                            slot.ReduceDeficit(_messageWeight);
                        }
                    }
                }
            }
        }, stoppingToken);
    }

    private IModel SetUpChannel()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        _connection = factory.CreateConnection();
        var channel = _connection.CreateModel();

        channel.BasicQos(prefetchSize: 0, prefetchCount: 100, global: false);

        return channel;
    }

    private List<PrioritySlot> SetUpPrioritySlots(SemaphoreSlim deliveriesToProcessSemaphore, IModel channel)
    {
        var slots = new List<PrioritySlot>();

        foreach (var queue in queueOptions.Queues)
        {
            channel.QueueDeclare(queue.Name, exclusive: false, autoDelete: false);
            var consumer = new PriorityConsumer(channel, _consumerFactory, deliveriesToProcessSemaphore, queue.Name);
            consumer.Start();

            slots.Add(new PrioritySlot(consumer, quantum: queue.Quantum));
        }

        return slots;
    }
}
