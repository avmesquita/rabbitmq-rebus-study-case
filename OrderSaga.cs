using Dapper;
using Rebus.Handlers;
using Rebus.Sagas;
using System.Data;

public class OrderSaga : Saga<OrderSagaData>,
    IAmInitiatedBy<OrderCreatedEvent>,
    IHandleMessages<PaymentReceivedEvent>
{
    private readonly IDbConnection _db;

    // Injeta o IDbConnection do Postgres registrado no Program.cs
    public OrderSaga(IDbConnection db)
    {
        _db = db;
    }

    protected override void CorrelateMessages(ICorrelationConfig<OrderSagaData> config)
    {
        config.Correlate<OrderCreatedEvent>(m => m.OrderId, d => d.OrderId);
        config.Correlate<PaymentReceivedEvent>(m => m.OrderId, d => d.OrderId);
    }

    public async Task Handle(OrderCreatedEvent message)
    {
        if (!IsNew) return;

        // 1. Atualiza o estado da Saga (Rebus salva na 'rebus_sagas')
        Data.OrderId = message.OrderId;
        Data.PaymentReceived = false;

        // 2. Persiste o pedido na tabela de negócio 'pedidos' via Dapper
        const string sql = @"
            INSERT INTO pedidos (id, valor, email_cliente, criado_em)
            VALUES (@OrderId, @Value, @CustomerEmail, NOW())
            ON CONFLICT (id) DO NOTHING;
        ";
        /*
            ON CONFLICT (id) 
            DO UPDATE SET valor = EXCLUDED.valor;        
        */

        await _db.ExecuteAsync(sql, message);

        Console.WriteLine($"[SAGA] Pedido {message.OrderId} gravado na tabela 'pedidos' e aguardando pagamento na 'rebus_sagas'!");
    }

    public async Task Handle(PaymentReceivedEvent message)
    {
        Console.WriteLine($"[SAGA] Pagamento do pedido {Data.OrderId} recebido!");
        
        // Finaliza o fluxo e remove o registro da tabela 'rebus_sagas'
        MarkAsComplete(); 
        await Task.CompletedTask;
    }
}
