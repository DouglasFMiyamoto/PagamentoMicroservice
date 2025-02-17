using MongoDB.Driver;
using PagamentoMicroservice.Core.Entities;

namespace PaymentMicroservice.Adapters.Database.MongoDB
{
    public class PagamentoContext
    {
        private readonly IMongoDatabase _database;
        private readonly string _connectionString = "mongodb://localhost:27017";

        public PagamentoContext(IConfiguration configuration)
        {
            // Utilizando a configuração para obter a string de conexão
            _connectionString = configuration.GetConnectionString("MongoDb");
            var client = new MongoClient(_connectionString);
            _database = client.GetDatabase("PagamentoDb");
        }

        // As coleções que o MongoDB vai gerenciar
        public IMongoCollection<Pagamento> Pagamentos => _database.GetCollection<Pagamento>("Pagamentos");
    }
}