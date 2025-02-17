namespace PagamentoMicroservice.Adapters.Messaging.Sqs.DTO
{
    public class PedidoMessage
    {
        public int Id { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
