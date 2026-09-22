# Estado Atual

## Objetivo

Demonstrar um fluxo de pedidos orientado a eventos usando API .NET 10, RabbitMQ, Rebus, PostgreSQL e uma saga persistente.

## Componentes

| Componente | Responsabilidade |
| --- | --- |
| `api/src` | Recebe chamadas HTTP e publica eventos de negócio. |
| `worker/src` | Consome eventos e executa `OrderSaga`. |
| `mailer/src` | Executa ciclos agendados de notificações. |
| `shared/StudyCase.Contracts` | Define os eventos compartilhados. |
| `shared/StudyCase.Domain` | Define a entidade `Order`. |
| PostgreSQL | Armazena pedidos e estado persistido das sagas. |
| RabbitMQ | Transporta as mensagens Rebus. |

## Contratos Atuais

Namespace: `StudyCase.Contracts`

- `OrderCreatedEvent(Guid OrderId, decimal Value, string CustomerEmail)`
- `PaymentReceivedEvent(Guid OrderId)`

## Fluxo de Negócio

```text
Cliente
  -> API: POST /orders
  -> RabbitMQ: OrderCreatedEvent
  -> Worker: OrderSaga
  -> PostgreSQL: pedidos + rebus_sagas

Cliente
  -> API: POST /orders/{orderId}/payment
  -> RabbitMQ: PaymentReceivedEvent
  -> Worker: OrderSaga correlacionada por OrderId
  -> Rebus: MarkAsComplete()

Mailer
  -> RabbitMQ: consome OrderNotificationRequested em mailer-queue
  -> PostgreSQL: registra a notificação de forma idempotente
```

## Ambientes

### Desenvolvimento

`docker-compose.yml` constrói API e worker localmente. O ambiente `dev-env` fornece VS Code via navegador, .NET 10 SDK e um daemon Docker-in-Docker persistente.

```bash
docker compose up -d --build
docker compose --project-directory . -f dev-env/docker-compose.development.yml up -d --build
```

### Estável

`docker-compose.stable.yml` usa as imagens publicadas `avmesquita/rebus-api:latest` e `avmesquita/rebus-worker:latest`.

```bash
docker compose -f docker-compose.stable.yml pull
docker compose -f docker-compose.stable.yml up -d
```

## Estado Conhecido

- API e worker compartilham a mesma biblioteca de contratos.
- A saga é propriedade do worker.
- A tabela `pedidos` é criada pelo worker no startup.
- As tabelas de saga são criadas pelo storage PostgreSQL do Rebus.
- A API fica na porta `8081` por padrão.
- O painel RabbitMQ fica na porta `15672`.
- A cobertura de testes Playwright ainda é inicial e precisa ser expandida.
- O Mailer consome `OrderNotificationRequested` e registra notificações em `email_notifications`, mas ainda não está conectado a um provedor SMTP ou API de e-mail.

## Próximos Passos

- Adicionar consulta do estado do pedido.
- Criar testes de integração do fluxo pedido/pagamento.
- Expandir os testes Playwright.
- Formalizar configuração de autenticação e credenciais no ambiente estável.
