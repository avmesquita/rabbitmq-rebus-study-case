using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using StudyCase.Contracts;
using StudyCase.Mailer.Services;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"};" +
                       $"Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"};" +
                       $"Database={Environment.GetEnvironmentVariable("DB_NAME") ?? "rebus"};" +
                       $"Username={Environment.GetEnvironmentVariable("DB_USER") ?? "teams"};" +
                       $"Password={Environment.GetEnvironmentVariable("DB_PASS") ?? "teams"}";

builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
builder.Services.AddSingleton<NotificationDispatcher>();

var rabbitConnectionString = $"amqp://{Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest"}:" +
                             $"{Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest"}@" +
                             $"{Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost"}:" +
                             $"{Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"}";

builder.Services.AutoRegisterHandlersFromAssemblyOf<NotificationDispatcher>();
builder.Services.AddRebus(configure => configure
    .Transport(transport => transport.UseRabbitMq(rabbitConnectionString, "mailer-queue"))
    .Routing(routing => routing.TypeBased()
        .Map<OrderNotificationRequested>("mailer-queue")));

var host = builder.Build();

await EnsureDatabaseCreatedAsync(connectionString);
await host.RunAsync();

static async Task EnsureDatabaseCreatedAsync(string connectionString)
{
    await using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();

    const string sql = """
        CREATE TABLE IF NOT EXISTS email_notifications (
            order_id UUID PRIMARY KEY,
            recipient VARCHAR(255) NOT NULL,
            status VARCHAR(32) NOT NULL,
            created_at TIMESTAMPTZ NOT NULL,
            sent_at TIMESTAMPTZ NULL
        );

        ALTER TABLE email_notifications
            DROP CONSTRAINT IF EXISTS email_notifications_order_id_fkey;
        """;

    await connection.ExecuteAsync(sql);
}
