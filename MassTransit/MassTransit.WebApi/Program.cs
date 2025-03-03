using System.Security.Authentication;
using System.Text.Json;
using MassTransit;
using MassTransit.Core;
using MassTransit.WebApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(
            host: builder.Configuration["RabbitMq:Host"],
            port: ushort.Parse(builder.Configuration["RabbitMq:Port"]!),
            virtualHost: builder.Configuration["RabbitMq:VirtualHost"],
            configure: h =>
            {
                h.Username(builder.Configuration["RabbitMq:Username"]!);
                h.Password(builder.Configuration["RabbitMq:Password"]!);
            });
        
        cfg.UseRawJsonSerializer(RawSerializerOptions.AnyMessageType);
        
        cfg.ConfigureJsonSerializerOptions(_ => new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        cfg.ConfigureEndpoints(context);
        
        cfg.Message<PaymentMessage>(m =>
        {
            m.SetEntityName("payment_events");
        });
        cfg.Publish<PaymentMessage>(p =>
        {
            p.ExchangeType = ExchangeType.Fanout;
        });
        cfg.Publish<Fault<PaymentMessage>>(p =>
        {
            p.Exclude = true;
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/publish-payment", async (IPublishEndpoint publishEndpoint) =>
    {
        var random = new Random();
        PaymentMessage message = new PaymentMessage
        {
            PaymentId = random.Next(1, 1000), Amount = Math.Round((decimal)random.NextDouble() * 100000, 2),
            Currency = "PLN"
        };

        await publishEndpoint.Publish(message);

        return Results.Ok("Payment message published");
    })
    .WithName("PublishPayment")
    .WithOpenApi();

await app.RunAsync();
