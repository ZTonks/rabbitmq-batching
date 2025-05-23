using RabbitMQ.Client;
using Microsoft.Extensions.Options;
using Messaging.RabbitMQ.Batched.Misc;
using Messaging.RabbitMQ.Batched.Options;

namespace Messaging.RabbitMQ.Batched;

public class MessageHandlerHostedService<T>(
    IOptions<RabbitMQOptions> rabbitMQOptions,
    IOptions<RabbitMQConsumerOptions> consumerOptions,
    T processor,
    ILogger<T> logger) : BackgroundService
        where T : class, IBatchProcessor
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batchSize = consumerOptions.Value.MaxMessageBatchSize;
        var maxBatchWaitTime = TimeSpan.FromMilliseconds(consumerOptions.Value.MaxBatchWaitTimeMs);

        var channel = SetUpChannel();
        var consumer = new BatchedConsumer(channel, consumerOptions);
        consumer.Start();

        while (!stoppingToken.IsCancellationRequested)
        {
            var sw = ValueStopwatch.StartNew();

            do
            {
                if (consumer.Semaphore.CurrentCount == 0
                        || sw.GetElapsedTime() > maxBatchWaitTime)
                {
                    break;
                }

                await Task.Delay(consumerOptions.Value.BufferTimeMs, stoppingToken);
            }
            while (true);

            if (consumer.Semaphore.CurrentCount == batchSize)
            {
                continue;
            }

            var thisBatch = new List<(ulong deliveryTag, byte[] body)>();

            while (consumer.Deliveries.TryDequeue(out var delivery))
            {
                thisBatch.Add(delivery);
                consumer.Semaphore.Release();
            }

            await Task.Factory.StartNew(async () =>
            {
                var success = false;
                var tryCount = 0;
                var wait = (decimal)consumerOptions.Value.MaxBatchWaitTimeMs;
                var isExponentialRetry = consumerOptions.Value.IsExponentialRetryStrategy;
                var hasRetryStrategy = consumerOptions.Value.IsExponentialRetryStrategy.HasValue;

                do
                {
                    try
                    {
                        await processor.ProcessMessages(thisBatch, stoppingToken);

                        var lastDelivery = thisBatch.Select(m => m.deliveryTag).Last();

                        channel.BasicAck(lastDelivery, multiple: true);

                        success = true;

                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, ex.Message);

                        if (hasRetryStrategy)
                        {
                            if (isExponentialRetry.GetValueOrDefault())
                            {
                                wait *= 1.25M;
                            }

                            await Task.Delay((int)wait);
                        }
                    }
                }
                while (++tryCount < consumerOptions.Value.MaxRetries.GetValueOrDefault(1));

                if (success)
                {
                    return;
                }

                // TODO other reject strategy?
                thisBatch.ForEach(m => channel.BasicNack(m.deliveryTag, multiple: false, requeue: false));
            },
            TaskCreationOptions.LongRunning);
        }
    }

    private IModel SetUpChannel()
    {
        var rabbitMQOptionsValue = rabbitMQOptions.Value;

        var factory = new ConnectionFactory
        {
            HostName = rabbitMQOptionsValue.ConsumerHostName,
            UserName = rabbitMQOptionsValue.ConsumerUserName,
            Password = rabbitMQOptionsValue.ConsumerPassword,
            Port = rabbitMQOptionsValue.Port,
            VirtualHost = rabbitMQOptionsValue.ConsumerVirtualHost,
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.BasicQos(
            prefetchSize: 0,
            prefetchCount: consumerOptions.Value.PrefetchCount,
            global: false);

        return channel;
    }
}
