using System.Data;
using Dapper;
using Rebus.Handlers;

public class OrderCreatedHandler : IHandleMessages<OrderCreatedEvent>
{
    private readonly IDbConnection _db;

    public OrderCreatedHandler(IDbConnection db)
    {
        _db = db;
    }
    public async Task Handle(OrderCreatedEvent message)
    {
        // Regra de negócio simples
        Console.WriteLine($"[LOG] Processando Pedido: {message.OrderId} no valor de R$ {message.Value}");

        const string sql = @"
            INSERT INTO pedidos (id, valor, email_cliente, criado_em)
            VALUES (@OrderId, @Value, @CustomerEmail, NOW());
        ";

        // Executa sem montar ORMs pesados em cima
        await _db.ExecuteAsync(sql, message);        
        
        await Task.CompletedTask;
    }
}
