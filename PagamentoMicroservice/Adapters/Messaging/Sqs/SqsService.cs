using Amazon.SQS;
using Amazon.SQS.Model;
using PagamentoMicroservice.Core.Ports;

namespace PedidoMicroservice.Adapters.Messaging.Sqs
{
    public class SqsService : ISqsService
    {
        private readonly IAmazonSQS _sqsClient;
        private string _queueUrl;
        private string _queueUrlPedidoPago;
        private const string QueueName = "pedido-criado";
        private const string QueueNamePedidoPago = "pedido-pago";

        public SqsService(IConfiguration configuration)
        {
            _sqsClient = new AmazonSQSClient(
                "test", "test",
                new AmazonSQSConfig { ServiceURL = "http://host.docker.internal:4566" }
            );

            InicializarFilas().GetAwaiter().GetResult();
        }

        private async Task InicializarFilas()
        {
            _queueUrl = await ObterOuCriarFilaAsync(QueueName);
            _queueUrlPedidoPago = await ObterOuCriarFilaAsync(QueueNamePedidoPago);
        }

        private async Task<string> ObterOuCriarFilaAsync(string queueName)
        {
            try
            {
                var response = await _sqsClient.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = queueName });
                return response.QueueUrl;
            }
            catch (QueueDoesNotExistException)
            {
                var createResponse = await _sqsClient.CreateQueueAsync(new CreateQueueRequest { QueueName = queueName });
                Console.WriteLine($"Fila '{queueName}' criada.");
                return createResponse.QueueUrl;
            }
        }

        public async Task<ReceiveMessageResponse> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = 20
            };

            return await _sqsClient.ReceiveMessageAsync(receiveMessageRequest, cancellationToken);
        }

        public async Task EnviarMensagemPedidoPagoAsync(string mensagem)
        {
            var request = new SendMessageRequest
            {
                QueueUrl = _queueUrlPedidoPago,
                MessageBody = mensagem
            };

            await _sqsClient.SendMessageAsync(request);
            Console.WriteLine($"Mensagem enviada para fila '{QueueNamePedidoPago}': {mensagem}");
        }

        public async Task ApagarMensagemAsync(string receiptHandle)
        {
            var deleteMessageRequest = new DeleteMessageRequest
            {
                QueueUrl = _queueUrl,
                ReceiptHandle = receiptHandle
            };

            await _sqsClient.DeleteMessageAsync(deleteMessageRequest);
            Console.WriteLine("Mensagem apagada da fila: pedido-criado");
        }

        public async Task EsperarLocalStackAsync()
        {
            int tentativas = 10;
            while (tentativas > 0)
            {
                try
                {
                    var response = await _sqsClient.ListQueuesAsync(new ListQueuesRequest());
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Console.WriteLine("✅ LocalStack está pronto.");
                        return;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"⏳ Aguardando LocalStack... ({tentativas} tentativas restantes)");
                }

                await Task.Delay(5000);
                tentativas--;
            }

            throw new Exception("❌ LocalStack não ficou pronto a tempo!");
        }
    }
}