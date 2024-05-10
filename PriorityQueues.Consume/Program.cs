using PriorityQueues.Consume;

var builder = Host.CreateApplicationBuilder();
builder.Services
    .AddSingleton<ConsumerFactory>()
    .Configure<QueueOptions>(builder.Configuration.GetSection("QueueOptions"))
    .AddHostedService<MessageHandlerHostedService>();

var host = builder.Build();

await host.RunAsync();
