# Microserviço de Pagamento

## Descrição
O microserviço de Pagamento é responsável por processar pagamentos de pedidos recebidos via Amazon SQS. Após a confirmação do pagamento, ele publica uma mensagem em outra fila para que o pedido possa ser processado pelo microserviço de produção.

## Fluxo do Pagamento
1. **Consumo da Fila `pedido-criado`**: O serviço consome mensagens da fila Amazon SQS `pedido-criado`.
2. **Processamento do Pagamento**: O pagamento é validado e processado.
3. **Publicação na Fila `pedido-pago`**: Após a confirmação do pagamento, uma mensagem é publicada na fila `pedido-pago`.
4. **Erros no Pagamento**: Caso o pagamento falhe, um tratamento adequado é aplicado, podendo incluir reprocessamento ou notificação.

## Tecnologias Utilizadas
- **.NET 8** (C#)
- **Amazon SQS** (Mensageria)
- **Docker** (Containerização)
- **LocalStack** (Simulação do AWS SQS em ambiente local para testes)

## Como Executar o Microserviço
### 1. Configurar Dependências
Certifique-se de que você tem os seguintes serviços configurados e em execução:
- **LocalStack (opcional para testes locais)**: Para simular o SQS.

### 2. Configurar Variáveis de Ambiente
Defina as variáveis necessárias no ambiente para conexão com o SQS.

Exemplo de configuração no `appsettings.json`:
```json
{
  "AWS": {
    "SQS": {
      "PedidoCriadoQueueUrl": "http://localhost:4566/000000000000/pedido-criado",
      "PedidoPagoQueueUrl": "http://localhost:4566/000000000000/pedido-pago"
    }
  }
}
```

### 3. Executar a Aplicação
Com o Docker:
```sh
docker-compose up --build
```
Ou localmente:
```sh
dotnet run
```

## Testes
Este microserviço conta com testes de unidade para garantir a qualidade do código. Para rodar os testes:
```sh
dotnet test
```
