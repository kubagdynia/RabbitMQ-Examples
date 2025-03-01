using MassTransit;
using MassTransit.ConsoleConsumer.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((builderContext, services) =>
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        services.AddMassTransit(x =>
        {
            x.AddConsumer<PaymentNotificationConsumer>();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(
                    host: builderContext.Configuration["RabbitMq:Host"],
                    port: ushort.Parse(builderContext.Configuration["RabbitMq:Port"]!),
                    virtualHost: builderContext.Configuration["RabbitMq:VirtualHost"],
                    configure: h =>
                    {
                        h.Username(builderContext.Configuration["RabbitMq:Username"]!);
                        h.Password(builderContext.Configuration["RabbitMq:Password"]!);
                    });
                
                cfg.UseRawJsonDeserializer(RawSerializerOptions.All);

                cfg.ReceiveEndpoint("payment-notification", e =>
                {
                    e.UseMessageRetry(r => r.Interval(
                        retryCount: 3, interval: TimeSpan.FromSeconds(5)));
                    
                    // e.UseMessageRetry(r => r.Incremental(
                    //     retryLimit: 5, // max retry count
                    //     initialInterval: TimeSpan.FromSeconds(10), // initial retry interval
                    //     intervalIncrement: TimeSpan.FromSeconds(5))); // retry interval increment
                    //
                    // e.UseMessageRetry(r => r.Exponential(
                    //     retryLimit: 5, // max retry count
                    //     minInterval: TimeSpan.FromSeconds(10), // initial retry interval
                    //     maxInterval: TimeSpan.FromSeconds(30), // max retry interval
                    //     intervalDelta: TimeSpan.FromSeconds(5))); // retry interval increment
                    
                    e.ConfigureConsumeTopology = false;
                    
                    e.ConfigureConsumer<PaymentNotificationConsumer>(context); // better for DI
                });
                
            });
        });
    })
    .Build();

Console.WriteLine("========> Consumer is running <======== Press Ctrl+C to stop");
await host.RunAsync();
