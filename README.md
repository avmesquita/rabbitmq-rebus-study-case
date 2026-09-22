# Ensaio de estudos do Rebus

[![CI](https://github.com/avmesquita/rabbitmq-rebus-study-case/actions/workflows/ci.yml/badge.svg)](https://github.com/avmesquita/rabbitmq-rebus-study-case/actions/workflows/ci.yml)
 [![Abrir no GitHub Codespaces](https://img.shields.io/badge/Abrir%20no-GitHub%20Codespaces-181717?logo=github)](https://codespaces.new/avmesquita/rabbitmq-rebus-study-case?quickstart=1)
---

## Abstract

Este projeto apresenta um estudo prático da integração entre RabbitMQ e Rebus em uma arquitetura orientada a eventos. O exemplo utiliza uma saga de pedidos para demonstrar o processamento assíncrono de mensagens, a correlação entre eventos, a persistência e a reidratação do estado no PostgreSQL, além do tratamento de falhas e do encaminhamento de mensagens para a fila de erro. O objetivo é tornar visíveis os principais conceitos e decisões envolvidos na implementação de processos distribuídos e de longa duração com .NET.

## Fluxo Saga

Em arquiteturas orientadas a eventos usando Rebus com Sagas, a saga funciona como um Orquestrador de Estado Persistente. Ela é uma máquina de estados que reage a mensagens da fila, grava o progresso no banco e decide o que fazer a seguir.

Aqui está o fluxo completo, ponta a ponta:

```text
[ Cliente / API ]
       │
       │ 1. Dispara HTTP / Evento
       ▼
 ┌───────────┐       2. Envia OrderCreatedEvent       ┌───────────────┐
 │   Fila    ├───────────────────────────────────────►│  OrderSaga    │
 └───────────┘                                        └───────┬───────┘
                                                              │
                                                              │ 3. Instancia & Grava Estado Inicial
                                                              ▼
                                                     ┌─────────────────┐
                                                     │   rebus_sagas   │ (Postgres)
                                                     │ Status:         │
                                                     │ "aguardando     │
                                                     │  pagamento"     │
                                                     └────────┬────────┘
                                                              │
                                                              │ 4. Aguarda evento externo
                                                              ▼
 ┌───────────┐       5. Envia PaymentReceivedEvent    ┌───────────────┐
 │   Fila    ├───────────────────────────────────────►│  OrderSaga    │
 └───────────┘                                        └───────┬───────┘
                                                              │
                                                              │ 6. Reidrata estado pelo CorrelationID
                                                              │    & Atualiza no Postgres
                                                              ▼
                                                     ┌─────────────────┐
                                                     │   rebus_sagas   │
                                                     │ Status:         │
                                                     │ "pago"          │
                                                     └────────┬────────┘
                                                              │
                                                              │ 7. MarkAsComplete()
                                                              ▼
                                                     ┌─────────────────┐
                                                     │ Deleta linha em │
                                                     │  rebus_sagas    │
                                                     └─────────────────┘
```

### Diagrama de Sequência

```mermaid
sequenceDiagram
    autonumber
    actor Cliente as Cliente / API
    participant Fila as Fila (Broker)
    participant Saga as OrderSaga
    participant Banco as Postgres (rebus_sagas)

    Cliente->>Fila: Dispara HTTP / Evento
    Fila->>Saga: Envia OrderCreatedEvent
    Note over Saga,Banco: Status: "aguardando pagamento"
    Saga->>Banco: Instancia & Grava Estado Inicial

    Note over Fila,Saga: 4. Aguarda evento externo

    Fila->>Saga: Envia PaymentReceivedEvent
    Saga->>Banco: Reidrata estado pelo CorrelationID
    Note over Saga,Banco: Status: "pago"
    Saga->>Banco: Atualiza estado no Postgres
    
    Saga->>Banco: MarkAsComplete() (Deleta linha)
```    

### Diagrama Top-Down

```mermaid
graph TD
    A[Cliente / API] -->|1. Dispara HTTP / Evento| B(Fila)
    B -->|2. Envia OrderCreatedEvent| C[OrderSaga]
    C -->|3. Instancia & Grava Estado Inicial| D[(rebus_sagas - Postgres<br/>Status: 'aguardando pagamento')]
    
    D -->|4. Aguarda evento externo| E(Fila)
    E -->|5. Envia PaymentReceivedEvent| F[OrderSaga]
    
    F -->|6. Reidrata estado pelo CorrelationID<br/>& Atualiza no Postgres| G[(rebus_sagas - Postgres<br/>Status: 'pago')]
    
    G -->|7. MarkAsComplete| H[Deleta linha em rebus_sagas]
```

## O Ciclo de Vida em 4 Passos

### 1. Início da Saga (`IAmInitiatedBy<OrderCreatedEvent>`)
* **O que acontece:** A mensagem de criação do pedido chega na fila.
* **O Rebus faz:** Cria um novo registro na tabela `rebus_sagas` no Postgres, contendo um JSON com os dados do processo (`OrderSagaData`).
* **Estado no banco:** A propriedade no JSON fica como `"aguardando pagamento"`. O Rebus salva a correlação (ex: `OrderId`) para saber como achar essa saga no futuro.
* **Ação:** A saga pode disparar um comando externo (ex: solicitar cobrança no gateway) e para de executar. O processo morre na memória e fica só gravado no banco.

### 2. O Período de Espera (Standby)
* A mensagem inicial já foi processada e removida da fila.
* O sistema não gasta CPU nem conexões. A ordem está em descanso dentro da tabela `rebus_sagas`.

### 3. Reidratação e Avanço (`IHandleMessages<PaymentReceivedEvent>`)
* **O que acontece:** Horas ou segundos depois, o gateway envia um webhook e sua aplicação joga o evento `PaymentReceivedEvent` na fila.
* **O Rebus faz:** 
  1. Lê a mensagem e pega o `OrderId`.
  2. Vai até a tabela `rebus_sagas`, busca a linha correlacionada a esse `OrderId`.
  3. Reidrata a saga na memória exatamente com o estado gravado antes (`"aguardando pagamento"`).
* **Ação:** A saga executa o método que trata a aprovação. Atualiza o status interno do JSON para `"pago"` e salva de volta na `rebus_sagas`.

### 4. Finalização (`MarkAsComplete()`)
* Quando a saga cumpre todo o ciclo (pedido entregue ou cancelado), o código chama `MarkAsComplete()`.
* **O Rebus faz:** Apaga a linha correspondente da tabela `rebus_sagas`. A tabela serve apenas para sagas em andamento; saga finalizada não deve ocupar espaço lá.

## O papel de cada peça

### 1. A Fila de error (a DLQ do Rebus)
* **Onde fica**: No broker de mensageria (ex: uma queue chamada error no RabbitMQ).
* **Como funciona**: Se um handler (ou saga) lança uma exceção não tratada ao processar uma mensagem, o Rebus aplica a regra de retentativas (retries).
* **O desfecho**: Se falhar por $N$ vezes seguidas (o padrão do Rebus são 5 tentativas), o Rebus desiste, tira a mensagem da fila original e faz o Publish/Send dessa mensagem exatamente para a fila chamada error no broker.

### 2. A Tabela rebus_sagas (o Storage do Rebus)
* **Onde fica**: No PostgreSQL.Como funciona: Guarda apenas os dados de negócio necessários para a saga lembrar onde parou (ex: {"Status": "aguardando pagamento", "OrderId": 123}).
* **O que acontece no erro**: Se a mensagem estoura o limite de erros e vai para a fila de `error`, a transação no banco sofre Rollback. A tabela `rebus_sagas` continua com a foto do último estado válido e não é alterada.

### Resumo do Comportamento
* A fila de `error` é o cemitério de mensagens (Dead Letter Queue) mantido no Broker.
* A tabela `rebus_sagas` é o bloco de notas do estado mantido no Banco.

Se a sua mensagem foi parar na fila de error, você precisa ir no painel do seu broker (ou via CLI/Rebus Fleet Manager) para inspecionar o payload e o stack trace do erro gravado nos cabeçalhos (headers) dessa mensagem.


## Execução

### Pré-requisitos

- Docker instalado e em execução
- Docker Compose v2

```bash
git clone https://github.com/avmesquita/rabbitmq-rebus-study-case.git
cd rabbitmq-rebus-study-case
chmod +x start.sh
./start.sh
```

### Execução

Após abrir o ambiente remoto, inicie a infraestrutura com:

```bash
docker compose up -d --build
```

### Ambiente de desenvolvimento isolado no VS Code

O ambiente de desenvolvimento roda o VS Code no navegador com .NET 10 SDK e
um daemon Docker isolado. A pasta raiz do repositório é aberta como `/workspace`.

Suba primeiro a infraestrutura e depois o ambiente de desenvolvimento:

```bash
docker compose up -d --build
docker compose --project-directory . -f dev-env/docker-compose.development.yml up -d --build
```

Abra `http://localhost:8443` e use a senha definida em `VSCODE_PASSWORD`
(o padrão é `devcontainer`). Para alterar a porta, use `VSCODE_PORT`.
Na primeira inicialização, o ambiente clona `REPOSITORY_URL` na referência
`REPOSITORY_REF` (por padrão, o repositório público e a branch `main`) para o
volume persistente `rebus_workspace`.

O serviço `vscode` acessa apenas o daemon Docker `docker` do compose, sem usar
o socket Docker do host. O serviço Docker-in-Docker requer `privileged` para
funcionar e mantém seus dados no volume `rebus_docker_data`.

A API fica disponível em `http://localhost:8081` por padrão. Para alterar a
porta publicada, use `API_PORT`.

## Publicação remota

O projeto também possui uma configuração Dev Container compatível com GitHub
Codespaces, Gitpod, DevPod, Coder e ambientes equivalentes em Azure, AWS ou
Google Cloud.

[![Abrir no GitHub Codespaces](https://img.shields.io/badge/Abrir%20no-GitHub%20Codespaces-181717?logo=github)](https://codespaces.new/avmesquita/rabbitmq-rebus-study-case?quickstart=1)

Mais detalhes estão em [`.devcontainer/README.md`](.devcontainer/README.md).

