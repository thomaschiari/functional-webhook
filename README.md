# Serviço de Webhook Funcional

Este projeto é uma implementação de um serviço de webhook para processamento de pagamentos.

## Descrição

O serviço escuta requisições HTTP POST de um gateway de pagamento no endpoint `/webhook`. Ele valida a requisição quanto à segurança e integridade dos dados, verifica transações duplicadas e, em seguida, confirma ou cancela a transação chamando uma API separada. O serviço é implementado em F# utilizando o framework web Giraffe e .NET 9.0.

## Pré-requisitos

- SDK .NET 9.0
- Python 3.9.X ou superior
- Bibliotecas Python: `fastapi`, `uvicorn`, `requests`

## Como Instalar e Executar o Serviço

1. Clone o repositório e navegue até o diretório do projeto.
2. Restaure as dependências:
   ```sh
   dotnet restore WebhookService
   ```
3. Compile o serviço:
   ```sh
   dotnet build WebhookService
   ```
4. Execute o serviço:
   ```sh
   dotnet run --project WebhookService
   ```
   O servidor iniciará e escutará em `http://localhost:5000`.

## Como Testar o Serviço

1. Com o serviço F# em execução, abra um **novo terminal**.
2. Navegue até o diretório `test`:
   ```sh
   cd test
   ```
3. (Opcional) Crie um ambiente virtual Python e ative-o:
   ```sh
   python3 -m venv env
   source env/bin/activate
   ```
4. Instale as dependências Python:
   ```sh
   pip install -r requirements.txt
   ```
5. Execute o script de teste:
   ```sh
   python3 test_webhook.py
   ```

O script executará uma série de testes contra o serviço em execução e imprimirá os resultados. Uma implementação bem-sucedida passará em todos os 6 testes:

```
✅ Confirmação recebida: {'transaction_id': 'abc123'}
1. Webhook test ok: successful!
2. Webhook test ok: transação duplicada!
❌ Cancelamento recebido: {'transaction_id': 'abc123a'}
3. Webhook test ok: amount incorreto!
4. Webhook test ok: Token Invalido!
5. Webhook test ok: Payload Invalido!
❌ Cancelamento recebido: {'transaction_id': 'abc123abc'}
6. Webhook test ok: Campos ausentes!
6/6 tests completed.
Confirmações recebidas: ['abc123']
Cancelamentos recebidos: ['abc123a', 'abc123abc']
```

## Estrutura do Projeto

- `WebhookService/` — Código fonte F# do serviço de webhook
- `test/` — Script de teste Python e requisitos