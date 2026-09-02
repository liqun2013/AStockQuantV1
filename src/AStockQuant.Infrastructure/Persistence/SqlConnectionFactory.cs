using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
namespace AStockQuant.Infrastructure.Persistence;
public sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection() => new SqlConnection(configuration.GetConnectionString("AStockQuant") ?? throw new InvalidOperationException("Connection string 'AStockQuant' is missing."));
}