# ADR-0001: Domínio e Contratos em Bibliotecas Compartilhadas

## Status

Aceita

## Contexto

A implementação inicial concentrava eventos, entidades e lógica de processamento no worker. Isso fazia a API depender de tipos duplicados ou de detalhes internos do worker e podia produzir assemblies incompatíveis na comunicação Rebus.

## Decisão

Separar os tipos reutilizáveis em duas bibliotecas:

- `StudyCase.Contracts`: eventos de integração compartilhados.
- `StudyCase.Domain`: entidades e regras de negócio reutilizáveis.

API e worker referenciam essas bibliotecas. O worker continua proprietário da `OrderSaga` e de `OrderSagaData`.

## Consequências Positivas

- API e worker usam a mesma definição dos eventos.
- O domínio pode ser reutilizado sem dependência de infraestrutura.
- A saga fica isolada como detalhe de orquestração e persistência.
- A estrutura explicita as fronteiras entre HTTP, domínio, mensagens e infraestrutura.

## Consequências Negativas

- Alterações nos contratos exigem recompilação dos consumidores.
- O build precisa incluir corretamente todos os projetos compartilhados.
- Mudanças incompatíveis podem afetar mensagens já publicadas.

## Regras Derivadas

- Não duplicar eventos em projetos executáveis.
- Não colocar tipos específicos do Rebus no domínio.
- Verificar os `ProjectReference` quando qualquer pasta de `shared` for movida.
