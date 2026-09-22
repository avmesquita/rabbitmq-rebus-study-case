# API

A API é a porta de entrada HTTP do estudo de caso. Ela recebe pedidos, valida os dados básicos e publica eventos no RabbitMQ por meio do Rebus.

## Endpoints

- `GET /health`: verifica se a aplicação está respondendo.
- `POST /orders`: cria um pedido e publica `OrderCreatedEvent`.
- `POST /orders/{orderId}/payment`: publica `PaymentReceivedEvent`.

## Estrutura

- `src/Api.csproj`: projeto .NET 10 da API.
- `src/Program.cs`: configuração HTTP, Rebus e endpoints.
- `src/Dockerfile`: imagem usada no ambiente Docker.
- `tests/Api.PlaywrightTests`: espaço para testes de ponta a ponta.

## Dependências importantes

A API usa `StudyCase.Domain` para criar a entidade `Order` e `StudyCase.Contracts` para publicar os eventos. Ela não deve referenciar tipos internos do worker, como `OrderSaga` ou `OrderSagaData`.

Para executar localmente, consulte o [`README.md`](../README.md) da raiz.
