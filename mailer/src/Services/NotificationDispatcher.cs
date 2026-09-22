using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using Rebus.Handlers;
using StudyCase.Contracts;
using StudyCase.Domain;

namespace StudyCase.Mailer.Services;

public sealed class NotificationDispatcher(
    NpgsqlDataSource dataSource,
    ILogger<NotificationDispatcher> log) : IHandleMessages<OrderNotificationRequested>
{
    public async Task Handle(OrderNotificationRequested message)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        var order = new Order(message.OrderId, message.Value, message.CustomerEmail);

        var inserted = await connection.ExecuteAsync("""
            INSERT INTO email_notifications (order_id, recipient, status, created_at, sent_at)
            VALUES (@OrderId, @Recipient, 'processed', NOW(), NULL)
            ON CONFLICT (order_id) DO NOTHING;
            """, new
        {
            OrderId = order.Id,
            Recipient = order.CustomerEmail
        });

        if (inserted == 1)
        {
            log.LogInformation("Notificação processada para o pedido {OrderId} e destinatário {Recipient}",
                order.Id, order.CustomerEmail);
        }
        else
        {
            log.LogInformation("Notificação já processada para o pedido {OrderId}", order.Id);
        }
    }
}
