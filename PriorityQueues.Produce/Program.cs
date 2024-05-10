using PriorityQueues.Produce;

var builder = Host.CreateApplicationBuilder();
builder.Services
    .AddHostedService<MessageProducerHostedService>();

var host = builder.Build();

await host.RunAsync();