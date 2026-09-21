using Rebus.Sagas;

public class OrderSagaData : ISagaData
{
    public Guid Id { get; set; }          // Chave interna do Rebus
    public int Revision { get; set; }     // Controle de concorrência
    
    public Guid OrderId { get; set; }     // Nosso identificador de negócio
    public bool PaymentReceived { get; set; }
}
