namespace MassTransit.ConsoleConsumer.Models;

public class PaymentMessage
{
    public int PaymentId { get; set; }
    
    public decimal Amount { get; set; }
    
    public string Currency { get; set; } = "PLN";
}