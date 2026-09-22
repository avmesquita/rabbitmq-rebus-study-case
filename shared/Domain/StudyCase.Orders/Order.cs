namespace StudyCase.Domain.Orders;

public sealed class Order
{
    public Guid Id { get; private set; }
    public decimal Value { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private Order()
    {
    }

    public Order(Guid id, decimal value, string customerEmail, DateTime? createdAt = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Order id is required.", nameof(id));
        }

        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Order value must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new ArgumentException("Customer email is required.", nameof(customerEmail));
        }

        Id = id;
        Value = value;
        CustomerEmail = customerEmail;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }
}