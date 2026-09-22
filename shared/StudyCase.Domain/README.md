# StudyCase.Domain

Biblioteca .NET com o modelo de negócio reutilizável do estudo de caso.

## Conteúdo atual

- `Orders/Order.cs`: entidade principal de pedido, com identificador, valor, e-mail do cliente e data de criação.

## Regra principal

O domínio deve permanecer independente de Rebus, RabbitMQ, PostgreSQL e ASP.NET. API e worker podem utilizá-lo, mas regras de negócio não devem ser colocadas em tipos específicos desses processos.

Quando novas regras ou entidades forem adicionadas, mantenha-as nesta biblioteca sempre que puderem ser reutilizadas por mais de uma aplicação.
