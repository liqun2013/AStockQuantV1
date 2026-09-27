using AStockQuant.Application.DTOs;
using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Application.Interfaces;

public interface IScoreModelRuleRepository
{
		Task<IReadOnlyList<ScoreModelWeightDto>> GetModelWeightsAsync(string modelCode, CancellationToken cancellationToken);
		Task<IReadOnlyList<InvestmentIndicatorRule>> GetIndicatorRulesAsync(string modelCode, CancellationToken cancellationToken);
}
