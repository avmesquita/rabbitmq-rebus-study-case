using Rebus.Config;
using Rebus.Bus;
using Rebus.Routing.TypeBased;
using StudyCase.Domain;
using StudyCase.Contracts;

var builder = WebApplication.CreateBuilder(args);

var rabbitConnectionString = $"amqp://{Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest"}:" +
                             $"{Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest"}@" +
                             $"{Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost"}:" +
                             $"{Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"}";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRebus((configure, _) => configure
    .Transport(transport => transport.UseRabbitMq(rabbitConnectionString, "api-queue"))
    .Routing(routing => routing.TypeBased()
        .Map<OrderCreatedEvent>("pedidos-queue")
        .Map<PaymentReceivedEvent>("pedidos-queue")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithTags("Health");

app.MapPost("/orders", async (CreateOrderRequest request, IBus bus, CancellationToken cancellationToken) =>
{
    if (request.Value <= 0 || string.IsNullOrWhiteSpace(request.CustomerEmail))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["value"] = request.Value <= 0 ? ["Value must be greater than zero."] : [],
            ["customerEmail"] = string.IsNullOrWhiteSpace(request.CustomerEmail)
                ? ["CustomerEmail is required."]
                : []
        });
    }

    var order = new Order(Guid.NewGuid(), request.Value, request.CustomerEmail);
    await bus.Send(new OrderCreatedEvent(order.Id, order.Value, order.CustomerEmail));

    return Results.Accepted($"/orders/{order.Id}", new
    {
        order.Id,
        status = "queued"
    });
})
.WithName("CreateOrder")
.WithTags("Orders")
.Produces(StatusCodes.Status202Accepted)
.ProducesValidationProblem();

app.MapPost("/orders/{orderId:guid}/payment", async (Guid orderId, IBus bus, CancellationToken cancellationToken) =>
{
    var payment = new PaymentReceivedEvent(orderId);
    await bus.Send(payment);

    return Results.Accepted($"/orders/{orderId}", new
    {
        payment.OrderId,
        status = "payment-queued"
    });
})
.WithName("ReceivePayment")
.WithTags("Orders")
.Produces(StatusCodes.Status202Accepted);

app.Run();

public sealed record CreateOrderRequest(decimal Value, string CustomerEmail);

public partial class Program;
