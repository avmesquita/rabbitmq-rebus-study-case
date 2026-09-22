# Ambiente Dev Container

Esta configuração permite abrir o projeto em ambientes de desenvolvimento remoto compatíveis com a especificação Dev Container, como GitHub Codespaces, Gitpod, DevPod, Coder e ambientes equivalentes em Azure, AWS ou Google Cloud.

## Como usar

Abra o repositório no ambiente escolhido ou use o botão no README da raiz. A plataforma construirá a imagem usando `dev-env/Dockerfile`, montará o repositório no caminho padrão `/workspaces/<nome-do-repositório>` e instalará o suporte ao Docker-in-Docker.

## Portas encaminhadas

- `8081`: API do estudo de caso.
- `8443`: VS Code via navegador, quando o Compose do `dev-env` estiver em execução.
- `15672`: painel de gerenciamento do RabbitMQ.

## Observação

O Dev Container fornece o ambiente de ferramentas. PostgreSQL e RabbitMQ continuam sendo iniciados pelo Compose do projeto:

```bash
docker compose up -d --build
```

O `devcontainer.json` usa o checkout fornecido pela plataforma; ele não clona o repositório novamente no volume `rebus_workspace`. O volume e o caminho `/workspace` pertencem somente ao Compose local em `dev-env/docker-compose.development.yml`.