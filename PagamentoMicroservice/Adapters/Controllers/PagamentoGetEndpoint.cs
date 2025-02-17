using FastEndpoints;
using PagamentoMicroservice.Core.UseCases;
using PagamentoMicroservice.Adapters.Controllers.Response;

namespace PagamentoMicroservice.Adapters.Controllers
{
    public class PagamentoGetEndpoint : EndpointWithoutRequest<PagamentoResponse>
    {
        private readonly PagamentoUseCase _pagamentoUseCase;

        public PagamentoGetEndpoint(PagamentoUseCase pagamentoUseCase)
        {
            _pagamentoUseCase = pagamentoUseCase;
        }

        public override void Configure()
        {
            Get("/pagamentos");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var pagamentos = await _pagamentoUseCase.ObterPagamentosPorPedidoIdAsync(1);

            PagamentoResponse response = new PagamentoResponse();
            response.Mensagem = "Sucesso";

            await SendAsync(response);
        }
    }
}
