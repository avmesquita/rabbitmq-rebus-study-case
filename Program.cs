using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Rebus.Bus;
using Rebus.Config;
using Rebus.PostgreSql.Sagas;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using System.Data;

var builder = Host.CreateApplicationBuilder(args);

// String de conexão
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=192.168.80.5;Port=5439;Database=rebus;Username=teams;Password=teams";

var rabbitConnectionString = builder.Configuration.GetConnectionString("RabbitMqConnection") 
    ?? "amqp://guest:guest@localhost:5672";

// Registra IDbConnection scoped (uma conexão por operação/mensagem)
builder.Services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(connectionString));

// 1. Registra APENAS a Saga (evita duplicar com o OrderCreatedHandler)
builder.Services.AutoRegisterHandlersFromAssemblyOf<OrderSaga>();

// 2. Configura o Rebus
builder.Services.AddRebus((configure, provider) => configure
    .Transport(t => t.UseRabbitMq(rabbitConnectionString, "pedidos-queue"))
    .Routing(r => r.TypeBased()
        .Map<OrderCreatedEvent>("pedidos-queue")
        .Map<PaymentReceivedEvent>("pedidos-queue")
    )
    .Sagas(s => s.StoreInPostgres(
        connectionString, 
        dataTableName: "rebus_sagas", 
        indexTableName: "rebus_saga_indexes", 
        automaticallyCreateTables: true
    ))
);

var host = builder.Build();

// Prepara a tabela de negócio
await EnsureDatabaseCreatedAsync(connectionString);

// 3. Inicia a aplicação/escritor em background
await host.StartAsync();

var bus = host.Services.GetRequiredService<IBus>();

// Teste de disparo da Saga
var orderId = Guid.NewGuid();
Console.WriteLine($"[TESTE] Enviando OrderCreatedEvent para OrderId: {orderId}");

await bus.Send(new OrderCreatedEvent(
    OrderId: orderId,
    Value: 250.50m,
    CustomerEmail: "cliente@email.com"
));

Console.WriteLine("Serviço rodando. Verifique a tabela 'rebus_sagas' no Postgres!");
Console.WriteLine("Pressione CTRL+C para fechar.");

// Bloqueia a execução e mantém escutando
await host.WaitForShutdownAsync();

static async Task EnsureDatabaseCreatedAsync(string connectionString)
{
    using var db = new NpgsqlConnection(connectionString);
    
    const string sql = @"
        CREATE TABLE IF NOT EXISTS pedidos (
            id UUID PRIMARY KEY,
            valor NUMERIC(18, 2) NOT NULL,
            email_cliente VARCHAR(255) NOT NULL,
            criado_em TIMESTAMPTZ NOT NULL
        );
    ";

    await db.ExecuteAsync(sql);
}
