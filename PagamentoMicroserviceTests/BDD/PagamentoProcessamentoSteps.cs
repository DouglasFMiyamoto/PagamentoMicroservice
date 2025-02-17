using Moq;
using PagamentoMicroservice.Adapters.Messaging.Sqs.DTO;
using PagamentoMicroservice.Core.Ports;
using PagamentoMicroservice.Core.UseCases;
using TechTalk.SpecFlow;

namespace PagamentoMicroserviceTests.BDD
{
    [Binding]
    public class PagamentoProcessamentoSteps
    {
        private readonly PagamentoUseCase _pagamentoUseCase;
        private readonly Mock<IPagamentoRepository> _pagamentoRepositoryMock;
        private readonly Mock<ISqsService> _sqsServiceMock;
        private PedidoMessage _pedidoMessage;

        public PagamentoProcessamentoSteps()
        {
            // Mockando as dependências
            _pagamentoRepositoryMock = new Mock<IPagamentoRepository>();
            _sqsServiceMock = new Mock<ISqsService>();

            // Instanciando o caso de uso
            _pagamentoUseCase = new PagamentoUseCase(_pagamentoRepositoryMock.Object, _sqsServiceMock.Object);
        }

        [Given(@"que existe um pedido com id (\d+) e cliente ""(.*)""")]
        public void DadoQueExisteUmPedidoComIdECliente(int pedidoId, string cliente)
        {
            _pedidoMessage = new PedidoMessage
            {
                Id = pedidoId,
                Cliente = cliente,
                Status = "Criado"
            };
        }

        [When(@"o pagamento do pedido for processado")]
        public async Task QuandoOPagamentoDoPedidoForProcessado()
        {
            // O código do método ProcessarPagamentoAsync deve ser executado.
            await _pagamentoUseCase.ProcessarPagamentoAsync(_pedidoMessage.Id, _pedidoMessage.Cliente);
        }

        [Then(@"o pagamento do pedido deve ser marcado como ""(.*)""")]
        public void EntaoOPagamentoDoPedidoDeveSerMarcadoComo(string status)
        {
            // Verificando se o pagamento foi marcado como "Pago"
            _pagamentoRepositoryMock.Verify(repo => repo.AtualizarStatusPagamentoAsync(It.IsAny<Guid>(), It.Is<bool>(b => b == true)), Times.Once);
        }

        [Then(@"uma mensagem de pedido pago deve ser enviada para a fila")]
        public void EntaoUmaMensagemDePedidoPagoDeveSerEnviadaParaAFila()
        {
            // Verificando se a mensagem foi enviada para a fila
            _sqsServiceMock.Verify(sqs => sqs.EnviarMensagemPedidoPagoAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
