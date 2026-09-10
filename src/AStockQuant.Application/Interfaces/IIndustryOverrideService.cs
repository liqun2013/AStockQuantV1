using AStockQuant.Domain.Screening;

namespace AStockQuant.Application.Interfaces;

public interface IIndustryOverrideService
{
		Task<IndustryRuleSet> GetRuleSetAsync(int scoreModelId, IReadOnlyCollection<string> industryCodes, CancellationToken cancellationToken = default);
}