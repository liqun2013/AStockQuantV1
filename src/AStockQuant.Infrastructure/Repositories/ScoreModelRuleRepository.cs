using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Domain.ValueObjects;
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

		public async Task<IReadOnlyList<InvestmentIndicatorRule>> GetIndicatorRulesAsync(string modelCode, CancellationToken cancellationToken)
		{
				var (indicatorTable, ruleTable) = modelCode.ToUpperInvariant() switch
				{
						"BUFFETT" => ("Buffett.Indicator", "Buffett.ScoreRule"),
						"GRAHAM" => ("Graham.Indicator", "Graham.ScoreRule"),
						"FISHER" => ("Fisher.Indicator", "Fisher.ScoreRule"),
						_ => throw new ArgumentOutOfRangeException(nameof(modelCode), modelCode, "Unknown investment model code.")
				};

				using var connection = connectionFactory.CreateConnection();
				var sql = $"""
SELECT indicator.IndicatorCode, indicator.Weight, indicator.MaxScore, rule.MinValue, rule.MaxValue, rule.Score, rule.RuleOrder
FROM {indicatorTable} indicator
INNER JOIN {ruleTable} rule ON rule.IndicatorId = indicator.IndicatorId
WHERE indicator.IsActive = 1
	AND indicator.IsQuantitative = 1
	AND rule.IsActive = 1
	AND (rule.RuleProfileId IS NULL OR rule.RuleProfileId =
		(SELECT ProfileId FROM Quant.RuleProfile WHERE ProfileCode = 'DEFAULT' AND IsActive = 1))
ORDER BY indicator.IndicatorCode, rule.RuleOrder;
""";

				return (await connection.QueryAsync<InvestmentIndicatorRule>(new CommandDefinition(
						sql,
						cancellationToken: cancellationToken))).AsList();
		}
}
