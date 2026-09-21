# Casos de Estudo RabbitMQ/Rebus

## Fluxo

Em arquiteturas orientadas a eventos usando Rebus com Sagas, a saga funciona como um Orquestrador de Estado Persistente. Ela é uma máquina de estados que reage a mensagens da fila, grava o progresso no banco e decide o que fazer a seguir.

Aqui está o fluxo completo, ponta a ponta:

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
 ┌───────────┐       5. Envia PaymentApprovedEvent    ┌───────────────┐
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

## O Ciclo de Vida em 4 Passos

### 1. Início da Saga (IAmInitiatedBy<OrderCreatedEvent>)
O que acontece: A mensagem de criação do pedido chega na fila.

O Rebus faz: Cria um novo registro na tabela rebus_sagas no Postgres, contendo um JSON com os dados do processo (OrderSagaData).

Estado no banco: A propriedade no JSON fica como "aguardando pagamento". O Rebus salva a correlação (ex: OrderId) para saber como achar essa saga no futuro.

Ação: A saga pode disparar um comando externo (ex: solicitar cobrança no gateway) e para de executar. O processo morre na memória e fica só gravado no banco.

### 2. O Período de Espera (Standby)
A mensagem inicial já foi processada e removida da fila.

O sistema não gasta CPU nem conexões. A ordem está em descanso dentro da tabela rebus_sagas.

### 3. Reidratação e Avanço (IHandleMessages<PaymentApprovedEvent>)
O que acontece: Horas ou segundos depois, o gateway envia um webhook e sua aplicação joga o evento PaymentApprovedEvent na fila.


O Rebus faz:

Lê a mensagem e pega o OrderId.

Vai até a tabela rebus_sagas, busca a linha correlacionada a esse OrderId.

Reidrata a saga na memória exatamente com o estado gravado antes ("aguardando pagamento").

Ação: A saga executa o método que trata a aprovação. Atualiza o status interno do JSON para "pago" ou "faturado" e salva de volta na rebus_sagas.

### 4. Finalização (MarkAsComplete())
Quando a saga cumpre todo o ciclo (pedido entregue ou cancelado), o código chama MarkAsComplete().

O Rebus faz: Apaga a linha correspondente da tabela rebus_sagas. A tabela serve apenas para sagas em andamento; saga finalizada não deve ocupar espaço lá.

