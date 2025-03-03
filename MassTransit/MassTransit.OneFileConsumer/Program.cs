
using System.Text.Json;
using MassTransit;
using MassTransit.Consumers;
using MassTransit.Models;
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
                    host: "localhost",
                    port: 5676,
                    virtualHost: "test",
                    configure: h =>
                    {
                        h.Username("admin");
                        h.Password("adminadmin");
                    });
                
                cfg.UseRawJsonSerializer(RawSerializerOptions.AnyMessageType);
                cfg.UseRawJsonDeserializer(RawSerializerOptions.AnyMessageType);
                
                cfg.ConfigureJsonSerializerOptions(_ => new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                cfg.ReceiveEndpoint("payment_processing", e =>
                {
                    e.UseMessageRetry(r => r.Interval(
                        retryCount: 3, interval: TimeSpan.FromSeconds(5)));
                    e.ConfigureConsumeTopology = false;
                    e.ConfigureConsumer<PaymentNotificationConsumer>(context); // better for DI
                });
                
            });
        });
    })
    .Build();
    
Console.WriteLine("========> Consumer is running <======== Press Ctrl+C to stop");
await host.RunAsync();

namespace MassTransit.Models
{
    public class PaymentMessage
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "PLN";
    }
}

namespace MassTransit.Consumers
{
    public class PaymentNotificationConsumer(ILogger<PaymentNotificationConsumer> logger) : IConsumer<PaymentMessage>
    {
        public async Task Consume(ConsumeContext<PaymentMessage> context)
        {
            var message = context.Message;
            logger.LogInformation("$+++++++++ Received payment: {PaymentId} with amount {Amount} {Currency}",
                message.PaymentId, message.Amount, message.Currency);
            await Task.CompletedTask;
        }
    }
}