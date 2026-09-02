using System.Data;
namespace AStockQuant.Infrastructure.Persistence;
public interface ISqlConnectionFactory { IDbConnection CreateConnection(); }