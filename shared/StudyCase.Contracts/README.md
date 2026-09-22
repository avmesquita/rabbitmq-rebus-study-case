# StudyCase.Contracts

Biblioteca .NET com os contratos de integração compartilhados entre a API e o worker.

## Eventos atuais

- `OrderCreatedEvent`: informa que um pedido foi criado.
- `PaymentReceivedEvent`: informa que um pagamento foi recebido para um pedido.

Namespace atual: `StudyCase.Contracts`.

## Regra principal

Os eventos devem existir em uma única assembly compartilhada. Não duplique esses tipos na API ou no worker, porque o Rebus usa o tipo e a assembly para serializar e desserializar as mensagens.

Alterações nos eventos exigem a recompilação dos dois consumidores e podem afetar mensagens já publicadas.
