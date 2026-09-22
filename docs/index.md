---
layout: home
title: RabbitMQ Rebus Study Case
---

# RabbitMQ Rebus Study Case

Estudo de caso de processamento assíncrono de pedidos com .NET 10, RabbitMQ, Rebus, PostgreSQL e uma saga persistente.

## Navegação

- [Estado atual](current-state.md): componentes, fluxo de negócio, ambientes e próximos passos.
- [Arquitetura](architecture.md): responsabilidades, dependências, mensageria e persistência.
- [Decisões arquiteturais](decisions/): decisões registradas sobre a organização do projeto.
- [API](https://github.com/avmesquita/rabbitmq-rebus-study-case/blob/main/api/README.md): endpoints HTTP e publicação dos eventos.
- [Worker](https://github.com/avmesquita/rabbitmq-rebus-study-case/blob/main/worker/README.md): processamento da saga de pedidos.
- [Contratos compartilhados](https://github.com/avmesquita/rabbitmq-rebus-study-case/blob/main/shared/StudyCase.Contracts/README.md): eventos usados pela API e pelo worker.
- [Domínio compartilhado](https://github.com/avmesquita/rabbitmq-rebus-study-case/blob/main/shared/StudyCase.Domain/README.md): entidade `Order` e regras reutilizáveis.
- [Ambiente remoto](https://github.com/avmesquita/rabbitmq-rebus-study-case/blob/main/.devcontainer/README.md): uso com Codespaces e plataformas Dev Container.

## Executar localmente

```bash
docker compose up -d --build
```

A API fica disponível em `http://localhost:8081` e o painel do RabbitMQ em `http://localhost:15672`.
