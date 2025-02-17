using Amazon.SQS.Model;

namespace PagamentoMicroservice.Core.Ports
{
    public interface ISqsService
    {
        Task<ReceiveMessageResponse> ReceiveMessageAsync(CancellationToken cancellationToken);
        Task ApagarMensagemAsync(string receiptHandle);
        Task EnviarMensagemPedidoPagoAsync(string mensagem);
        Task EsperarLocalStackAsync();
    }
}
