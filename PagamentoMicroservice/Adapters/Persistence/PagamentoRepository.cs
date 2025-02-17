using MongoDB.Driver;
using PagamentoMicroservice.Core.Entities;
using PagamentoMicroservice.Core.Ports;

namespace PaymentMicroservice.Adapters.Database.MongoDB
{
    public class PagamentoRepository : IPagamentoRepository
    {
        private readonly PagamentoContext _context;

        public PagamentoRepository(PagamentoContext context)
        {
            _context = context;
        }

        public async Task AdicionarPagamentoAsync(Pagamento pagamento)
        {
            await _context.Pagamentos.InsertOneAsync(pagamento);
        }

        public async Task<Pagamento> ObterPagamentoPorIdAsync(Guid pagamentoId)
        {
            return await _context.Pagamentos
                .Find(p => p.Id == pagamentoId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Pagamento>> ObterPagamentosPorPedidoIdAsync(int pedidoId)
        {
            return await _context.Pagamentos
                .Find(p => p.PedidoId == pedidoId)
                .ToListAsync();
        }

        public async Task AtualizarStatusPagamentoAsync(Guid pagamentoId, bool pago)
        {
            var filter = Builders<Pagamento>.Filter.Eq(p => p.Id, pagamentoId);
            var update = Builders<Pagamento>.Update.Set(p => p.Pago, pago);
            await _context.Pagamentos.UpdateOneAsync(filter, update);
        }
    }
}
