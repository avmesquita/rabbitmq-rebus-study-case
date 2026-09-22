using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using System.Data;
using StudyCase.Worker.Orders;
using StudyCase.Domain.Orders;
using StudyCase.Contracts;


var builder = Host.CreateApplicationBuilder(args);

#region [ Configurações do Postgres e RabbitMQ ]

var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"};" +
                   $"Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"};" +
                   $"Database={Environment.GetEnvironmentVariable("DB_NAME") ?? "rebus"};" +
                   $"Username={Environment.GetEnvironmentVariable("DB_USER") ?? "rebus"};" +
                   $"Password={Environment.GetEnvironmentVariable("DB_PASS") ?? "rebus_pass"}";

var rabbitConnectionString = $"amqp://{Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest"}:" +
                       $"{Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest"}@" +
                       $"{Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost"}:" +
                       $"{Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"}";
#endregion

#region [ Registra IDbConnection scoped (uma conexão por operação/mensagem) ] 
builder.Services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(connectionString));
#endregion

#region [ Registra o Rebus e a Saga ]
builder.Services.AutoRegisterHandlersFromAssemblyOf<OrderSaga>();
#endregion

#region [ Configura o Rebus ]
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
#endregion

var host = builder.Build();

#region [ Prepara as tabelas de negócio (migrations) ]
await EnsureDatabaseCreatedAsync(connectionString);
#endregion

#region [ Inicia a aplicação/escritor em background ]
await host.StartAsync();
#endregion

var bus = host.Services.GetRequiredService<IBus>();

#region Test (Dispara o teste da saga) | DevDebugOption
/*
await TestSagaAsync(bus);
Console.WriteLine("Serviço rodando. Verifique a tabela 'rebus_sagas' no Postgres!");
*/
#endregion

Console.WriteLine("Pressione CTRL+C para fechar.");

// Bloqueia a execução e mantém escutando
await host.WaitForShutdownAsync();

#region [ Funções auxiliares ]

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

#region DevDebugOption
/*
static async Task TestSagaAsync(IBus bus)
{
    var orderId = Guid.NewGuid();
    var orderCreatedEvent = new OrderCreatedEvent(orderId, 100.0m, "cliente@email.com");

    Console.WriteLine($"[TESTE] Enviando OrderCreatedEvent para OrderId: {orderId}");
    await bus.Send(orderCreatedEvent);
}
*/
#endregion

#endregion
