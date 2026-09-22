# Mailer

Worker .NET 10 responsável por executar ciclos agendados de notificações.

## Como funciona

- `NotificationDispatcher` consome `OrderNotificationRequested` da fila `mailer-queue`.
- Cada mensagem gera um registro idempotente em `email_notifications`.
- O envio real de e-mail ainda não está conectado; o handler atual registra/loga o processamento e deixa a integração com SMTP/provider isolada para o próximo passo.

Múltiplas instâncias do Mailer podem consumir a mesma fila Rebus em paralelo.

## Executar

```bash
dotnet run --project mailer/src/Mailer.csproj
```

Para escalar os consumidores:

```bash
docker compose up -d --build --scale rebus_mailer=3
```
