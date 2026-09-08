using AStockQuant.Application.DTOs;

namespace AStockQuant.Application.Interfaces;

public interface IScoreModelRuleRepository
{
		Task<IReadOnlyList<ScoreModelWeightDto>> GetModelWeightsAsync(string modelCode, CancellationToken cancellationToken);
}
