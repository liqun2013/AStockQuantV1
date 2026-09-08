using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Infrastructure.Persistence;
using Dapper;

namespace AStockQuant.Infrastructure.Repositories;

public sealed class ScoreModelRuleRepository(ISqlConnectionFactory connectionFactory) : IScoreModelRuleRepository
{
		public async Task<IReadOnlyList<ScoreModelWeightDto>> GetModelWeightsAsync(string modelCode, CancellationToken cancellationToken)
		{
				using var connection = connectionFactory.CreateConnection();
				const string sql = """
SELECT weight.ComponentCode, weight.Weight
FROM Quant.ScoreModel model
INNER JOIN Quant.ScoreModelWeight weight ON weight.ModelId = model.ModelId
WHERE model.ModelCode = @ModelCode
	AND model.IsActive = 1
ORDER BY weight.ComponentCode;
""";

				return (await connection.QueryAsync<ScoreModelWeightDto>(new CommandDefinition(
						sql,
						new { ModelCode = modelCode },
						cancellationToken: cancellationToken))).AsList();
		}
}
