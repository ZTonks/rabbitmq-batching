using ConsumerTest;
using Messaging.RabbitMQ.Batched.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .ConfigureBatchedMessageHandler<DummyProcessor>(
        configureDefaultBatchProcessor: true);

var host = builder.Build();

await host.RunAsync();
