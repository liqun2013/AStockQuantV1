using AStockQuant.Domain.Screening;

namespace AStockQuant.Application.Interfaces;

public interface IIndustryOverrideRepository
{
		Task<IReadOnlyList<IndustryScreeningRuleOverride>> GetScreeningRuleOverridesAsync(int scoreModelId, IReadOnlyCollection<string> industryCodes, CancellationToken cancellationToken);
}