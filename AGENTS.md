# Contexto para Agentes

## Objetivo

Este repositório é um estudo de caso de processamento assíncrono de pedidos com .NET 10, RabbitMQ, Rebus, PostgreSQL e uma saga persistente.

## Estrutura

- `api/src`: API HTTP que valida pedidos e publica eventos.
- `worker/src`: processo Rebus que consome eventos e hospeda `OrderSaga`.
- `shared/StudyCase.Contracts`: eventos compartilhados entre API e worker.
- `shared/StudyCase.Domain`: entidades e regras de domínio reutilizáveis.
- `postgres`: script de inicialização do banco.
- `dev-env`: ambiente VS Code no navegador com .NET 10 e Docker-in-Docker.
- `docker-compose.yml`: ambiente local com build das imagens.
- `docker-compose.stable.yml`: ambiente usando imagens publicadas.

## Regras de Dependência

- API e worker podem referenciar `StudyCase.Contracts` e `StudyCase.Domain`.
- Eventos de integração ficam em `StudyCase.Contracts`.
- Entidades e regras de negócio ficam em `StudyCase.Domain`.
- `OrderSaga` e `OrderSagaData` são detalhes do worker/Rebus e não devem ser movidos para o domínio.
- A API não deve referenciar tipos internos do worker.
- Não duplicar eventos em API e worker: ambos precisam usar a mesma assembly e os mesmos namespaces.

## Fluxo Atual

1. `POST /orders` cria um `Order` e publica `OrderCreatedEvent`.
2. O worker recebe o evento e inicia `OrderSaga`.
3. A saga persiste o pedido em `pedidos` e seu estado em `rebus_sagas`.
4. `POST /orders/{orderId}/payment` publica `PaymentReceivedEvent`.
5. A saga correlaciona o pagamento por `OrderId` e chama `MarkAsComplete()`.

## Comandos de Validação

```bash
dotnet build api/src/Api.csproj
dotnet build worker/src/RebusExemplo.csproj
docker compose up -d --build
docker compose --project-directory . -f dev-env/docker-compose.development.yml up -d --build
docker compose -f docker-compose.stable.yml config
```

## Cuidados

- Preserve alterações locais existentes antes de editar.
- Verifique os `ProjectReference` após mover projetos em `shared`.
- Ao alterar eventos, compile API e worker juntos.
- Mudanças no contrato dos eventos podem impedir a desserialização de mensagens já publicadas.
- Não use o socket Docker do host no ambiente de desenvolvimento; o Compose usa um daemon Docker isolado.

## Documentação Relacionada

- Estado atual: `docs/current-state.md`
- Arquitetura: `docs/architecture.md`
- Decisões: `docs/decisions/`
