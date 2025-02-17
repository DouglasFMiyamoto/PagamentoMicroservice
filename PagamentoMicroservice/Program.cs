using FastEndpoints;
using MongoDB.Driver;
using PagamentoMicroservice.Core.Ports;
using PagamentoMicroservice.Core.UseCases;
using PedidoMicroservice.Adapters.Messaging.Sqs;
using Amazon.SQS;
using PaymentMicroservice.Adapters.Database.MongoDB;

var builder = WebApplication.CreateBuilder(args);

// Configuração de MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var mongoUrl = builder.Configuration.GetConnectionString("MongoDb");
    return new MongoClient(mongoUrl);
});

builder.Services.AddScoped<PagamentoContext>();
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
builder.Services.AddScoped<PagamentoUseCase>();

// Configuração do SQS
builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    return new AmazonSQSClient(
        "test", "test", // Credenciais para o LocalStack
        new AmazonSQSConfig { ServiceURL = "http://host.docker.internal:4566" }
    );
});

// Configurar o serviço para escutar a fila de SQS - Alterado para Scoped
builder.Services.AddScoped<ISqsService, SqsService>();

// FastEndpoints - Configuração dos endpoints
builder.Services.AddFastEndpoints();

// Adicionar suporte ao Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração de Log
builder.Logging.AddConsole();

var app = builder.Build();

// Aguarde LocalStack antes de iniciar a escuta da fila
var cancellationTokenSource = new CancellationTokenSource();
app.Lifetime.ApplicationStarted.Register(async () =>
{
    using (var scope = app.Services.CreateScope())
    {
        var sqsService = scope.ServiceProvider.GetRequiredService<ISqsService>();
        await sqsService.EsperarLocalStackAsync(); // Aguarda o LocalStack ficar pronto

        var listenerService = scope.ServiceProvider.GetRequiredService<PagamentoUseCase>();
        await listenerService.StartListeningAsync(cancellationTokenSource.Token);
    }
});

// Configuração do pipeline de requisição HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configuração do FastEndpoints
app.UseFastEndpoints();

app.Run();