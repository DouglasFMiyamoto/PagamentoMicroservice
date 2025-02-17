using MongoDB.Bson.Serialization.Attributes;

namespace PagamentoMicroservice.Core.Entities
{
    public class Pagamento
    {
        [BsonId]
        public Guid Id { get; set; }

        [BsonRequired] 
        public int PedidoId { get; set; }


        [BsonRequired]
        public string Cliente { get; set; } = string.Empty;

        [BsonRequired]
        public DateTime DataPagamento { get; set; }

        [BsonRequired]
        public DateTime DataAtualizacao { get; set; }

        [BsonRequired]
        public bool Pago { get; set; }
    }
}
