namespace StudyCase.Contracts;

public sealed record OrderCreatedEvent(Guid OrderId, decimal Value, string CustomerEmail);

public sealed record PaymentReceivedEvent(Guid OrderId);