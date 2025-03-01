using MassTransit.ConsoleConsumer.Models;
using Microsoft.Extensions.Logging;

namespace MassTransit.ConsoleConsumer.Consumers;

public class PaymentNotificationConsumer(ILogger<PaymentNotificationConsumer> logger) : IConsumer<PaymentMessage>
{
    public async Task Consume(ConsumeContext<PaymentMessage> context)
    {
        PaymentMessage message = context.Message;
        
        // throw new NotSupportedException("This is a test exception");
        
        logger.LogInformation("Received payment: {PaymentId} with amount {Amount} {Currency}",
            message.PaymentId, message.Amount, message.Currency);

        Console.WriteLine($"+++++++++ Payment notification received: {message.PaymentId}, {message.Amount} {message.Currency}");
        
        await Task.CompletedTask;
    }
}