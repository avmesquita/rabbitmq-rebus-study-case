# Ambiente de Desenvolvimento

Esta pasta contém o ambiente de desenvolvimento baseado em VS Code no navegador.

## O que ele fornece

- .NET 10 SDK.
- code-server, acessível pelo navegador.
- Git para clonar o repositório no primeiro uso.
- Docker CLI.
- Daemon Docker-in-Docker isolado do Docker do host.

## Como iniciar

A partir da raiz do repositório:

```bash
docker compose --project-directory . -f dev-env/docker-compose.development.yml up -d --build
```

Acesse `http://localhost:8443`. A senha padrão é `devcontainer` e pode ser alterada com `VSCODE_PASSWORD`.

## Workspace

O repositório é clonado no volume persistente `rebus_workspace`. A URL e a referência podem ser configuradas com `REPOSITORY_URL` e `REPOSITORY_REF`.

O daemon Docker interno usa volumes próprios e não monta o socket Docker do host.
