using AStockQuant.Application.Interfaces;
using AStockQuant.Domain.Screening;
using AStockQuant.Infrastructure.Persistence;
using Dapper;

namespace AStockQuant.Infrastructure.Repositories;

public sealed class IndustryOverrideRepository(ISqlConnectionFactory connectionFactory) : IIndustryOverrideRepository
{
		public async Task<IReadOnlyList<IndustryScreeningRuleOverride>> GetScreeningRuleOverridesAsync(int scoreModelId, IReadOnlyCollection<string> industryCodes, CancellationToken cancellationToken)
		{
				if (industryCodes.Count == 0) return [];

				using var connection = connectionFactory.CreateConnection();
				const string sql = """
SELECT industry.IndustryCode, override.RuleCode, override.NumericValue
FROM Strategy.IndustryScreeningRuleOverride override
INNER JOIN Basic.Industry industry ON industry.IndustryId = override.IndustryId
WHERE override.ScoreModelId = @ScoreModelId
	AND override.IsActive = 1
	AND industry.IndustryCode IN @IndustryCodes;
""";
				return (await connection.QueryAsync<IndustryScreeningRuleOverride>(new CommandDefinition(sql, new
				{
						ScoreModelId = scoreModelId,
						IndustryCodes = industryCodes
				}, cancellationToken: cancellationToken))).AsList();
		}
}