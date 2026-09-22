namespace RebusExemplo.Orders;

public record OrderCreatedEvent(Guid OrderId, decimal Value, string CustomerEmail);
