# Worker

O worker é o processo responsável por consumir mensagens do RabbitMQ e executar a saga persistente de pedidos usando Rebus.

## Fluxo

1. Recebe `OrderCreatedEvent`.
2. Inicia ou recupera `OrderSaga`.
3. Persiste o pedido em PostgreSQL.
4. Aguarda `PaymentReceivedEvent` correlacionado por `OrderId`.
5. Finaliza a saga com `MarkAsComplete()`.

## Estrutura

- `src/Program.cs`: configura PostgreSQL, RabbitMQ, Rebus e o storage da saga.
- `src/Orders/OrderSaga.cs`: lógica da saga.
- `src/Orders/OrderSagaData.cs`: estado persistido pelo Rebus.
- `src/Dockerfile`: imagem do worker.

## Fronteiras

O worker pode usar `StudyCase.Contracts` e `StudyCase.Domain`, mas é o único proprietário de `OrderSaga` e `OrderSagaData`. A API não deve depender dessas classes internas.

Para validar o projeto:

```bash
dotnet build worker/src/RebusExemplo.csproj
```
