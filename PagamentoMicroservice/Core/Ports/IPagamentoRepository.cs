using PagamentoMicroservice.Core.Entities;

namespace PagamentoMicroservice.Core.Ports
{
    public interface IPagamentoRepository
    {
        Task AdicionarPagamentoAsync(Pagamento pagamento);
        Task<Pagamento> ObterPagamentoPorIdAsync(Guid pagamentoId);
        Task<List<Pagamento>> ObterPagamentosPorPedidoIdAsync(int pedidoId);
        Task AtualizarStatusPagamentoAsync(Guid pagamentoId, bool pago);
    }
}
