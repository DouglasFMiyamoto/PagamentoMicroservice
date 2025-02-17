using Moq;
using PagamentoMicroservice.Core.Ports;
using PagamentoMicroservice.Core.UseCases;
using Xunit;
using PagamentoMicroservice.Adapters.Messaging.Sqs.DTO;
using Amazon.SQS.Model;
using PagamentoMicroservice.Core.Entities;
using FluentAssertions;

namespace PagamentoMicroserviceTests.UseCases
{
    public class PagamentoUseCaseTests
    {
        private readonly Mock<IPagamentoRepository> _mockPagamentoRepository;
        private readonly Mock<ISqsService> _mockSqsService;
        private readonly PagamentoUseCase _pagamentoUseCase;

        public PagamentoUseCaseTests()
        {
            _mockPagamentoRepository = new Mock<IPagamentoRepository>();
            _mockSqsService = new Mock<ISqsService>();
            _pagamentoUseCase = new PagamentoUseCase(_mockPagamentoRepository.Object, _mockSqsService.Object);
        }

        [Fact]
        public async Task StartListeningAsync_ShouldProcessMessages_WhenMessagesAreReceived()
        {
            // Arrange
            var pedidoMessage = new PedidoMessage
            {
                Id = 1,
                Cliente = "ClienteTest",
                Status = "Criado"
            };

            var message = new Amazon.SQS.Model.Message { Body = Newtonsoft.Json.JsonConvert.SerializeObject(pedidoMessage), ReceiptHandle = "handle1" };
            var response = new ReceiveMessageResponse { Messages = new List<Amazon.SQS.Model.Message> { message } };

            _mockSqsService.Setup(s => s.ReceiveMessageAsync(It.IsAny<CancellationToken>())).ReturnsAsync(response);
            _mockSqsService.Setup(s => s.ApagarMensagemAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var cancellationToken = new CancellationToken();
            await _pagamentoUseCase.StartListeningAsync(cancellationToken);

            // Assert
            _mockSqsService.Verify(s => s.ReceiveMessageAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockSqsService.Verify(s => s.ApagarMensagemAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessarPagamentoAsync_ShouldProcessAndSendMessage()
        {
            // Arrange
            var pagamentoId = 1;
            var cliente = "ClienteTest";
            var pagamento = new Pagamento
            {
                Id = Guid.NewGuid(),
                PedidoId = pagamentoId,
                Cliente = cliente,
                Pago = false,
                DataPagamento = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            var mensagemEsperada = $"{{ \"id\": {pagamento.PedidoId}, \"cliente\": {pagamento.Cliente}, \"pago\": {pagamento.Pago} }}";

            _mockPagamentoRepository.Setup(repo => repo.AdicionarPagamentoAsync(It.IsAny<Pagamento>())).Returns(Task.CompletedTask);
            _mockPagamentoRepository.Setup(repo => repo.AtualizarStatusPagamentoAsync(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(Task.CompletedTask);
            _mockSqsService.Setup(sqs => sqs.EnviarMensagemPedidoPagoAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            await _pagamentoUseCase.ProcessarPagamentoAsync(pagamentoId, cliente);

            // Assert
            _mockPagamentoRepository.Verify(repo => repo.AdicionarPagamentoAsync(It.IsAny<Pagamento>()), Times.Once);
            _mockPagamentoRepository.Verify(repo => repo.AtualizarStatusPagamentoAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
            _mockSqsService.Verify(sqs => sqs.EnviarMensagemPedidoPagoAsync(It.Is<string>(msg => msg == mensagemEsperada)), Times.Once);
        }

        [Fact]
        public async Task ObterPagamentosPorPedidoIdAsync_ShouldReturnList_WhenPaymentsExist()
        {
            // Arrange
            var pedidoId = 1;
            var pagamentos = new List<Pagamento>
            {
                new Pagamento
                {
                    Id = Guid.NewGuid(),
                    PedidoId = pedidoId,
                    Cliente = "ClienteTest",
                    Pago = true,
                    DataPagamento = DateTime.UtcNow,
                    DataAtualizacao = DateTime.UtcNow
                }
            };

            _mockPagamentoRepository.Setup(repo => repo.ObterPagamentosPorPedidoIdAsync(pedidoId)).ReturnsAsync(pagamentos);

            // Act
            var result = await _pagamentoUseCase.ObterPagamentosPorPedidoIdAsync(pedidoId);

            // Assert
            result.Should().NotBeEmpty();
            result.Count.Should().Be(1);
            result[0].PedidoId.Should().Be(pedidoId);
        }

        [Fact]
        public async Task ObterPagamentoPorIdAsync_ShouldReturnPayment_WhenPaymentExists()
        {
            // Arrange
            var pagamentoId = Guid.NewGuid();
            var pagamento = new Pagamento
            {
                Id = pagamentoId,
                PedidoId = 1,
                Cliente = "ClienteTest",
                Pago = true,
                DataPagamento = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            _mockPagamentoRepository.Setup(repo => repo.ObterPagamentoPorIdAsync(pagamentoId)).ReturnsAsync(pagamento);

            // Act
            var result = await _pagamentoUseCase.ObterPagamentoPorIdAsync(pagamentoId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(pagamentoId);
        }
    }
}
