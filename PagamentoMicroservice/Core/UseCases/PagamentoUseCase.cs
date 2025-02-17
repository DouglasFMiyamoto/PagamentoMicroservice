using PagamentoMicroservice.Adapters.Messaging.Sqs.DTO;
using PagamentoMicroservice.Core.Entities;
using PagamentoMicroservice.Core.Ports;

namespace PagamentoMicroservice.Core.UseCases
{
    public class PagamentoUseCase
    {
        private readonly IPagamentoRepository _pagamentoRepository;
        private readonly ISqsService _sqsService; 

        public PagamentoUseCase(IPagamentoRepository pagamentoRepository, ISqsService sqsService)
        {
            _pagamentoRepository = pagamentoRepository;
            _sqsService = sqsService;
        }

        public async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(30000);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var response = await _sqsService.ReceiveMessageAsync(cancellationToken);
                   
                    foreach (var message in response.Messages)
                    {
                        var pedidoMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<PedidoMessage>(message.Body);
                        await ProcessarPagamentoAsync(pedidoMessage.Id, pedidoMessage.Cliente.ToString());
                        await _sqsService.ApagarMensagemAsync(message.ReceiptHandle);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao consumir mensagens da fila: {ex.Message}");
                    await Task.Delay(5000); // Aguarda antes de tentar novamente
                }
            }
        }

        public async Task ProcessarPagamentoAsync(int pedidoId, string cliente)
        {            
            var pagamento = new Pagamento
            {
                Id = Guid.NewGuid(),
                PedidoId = pedidoId,
                Cliente = cliente,
                Pago = false,
                DataPagamento = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            Console.WriteLine("Processando o pagamento...");
            //await _pagamentoRepository.AdicionarPagamentoAsync(pagamento);

            pagamento.Pago = true; 
            //await _pagamentoRepository.AtualizarStatusPagamentoAsync(pagamento.Id, pagamento.Pago);


            var mensagem = $"{{ \"id\": {pagamento.PedidoId}, \"cliente\": {pagamento.Cliente.ToString()}, \"pago\": {pagamento.Pago} }}";
            await _sqsService.EnviarMensagemPedidoPagoAsync(mensagem);
        }

        public async Task<List<Pagamento>> ObterPagamentosPorPedidoIdAsync(int pedidoId)
        {
            return await _pagamentoRepository.ObterPagamentosPorPedidoIdAsync(pedidoId);
        }

        public async Task<Pagamento> ObterPagamentoPorIdAsync(Guid pagamentoId)
        {
            return await _pagamentoRepository.ObterPagamentoPorIdAsync(pagamentoId);
        }
    }
}
