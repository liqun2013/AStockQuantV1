using AStockQuant.Application.Interfaces;
using AStockQuant.Domain.Screening;

namespace AStockQuant.Application.Services;

public sealed class IndustryOverrideService(IIndustryOverrideRepository repository) : IIndustryOverrideService
{
		public async Task<IndustryRuleSet> GetRuleSetAsync(int scoreModelId, IReadOnlyCollection<string> industryCodes, CancellationToken cancellationToken = default)
		{
				var normalizedCodes = industryCodes
						.Where(code => !string.IsNullOrWhiteSpace(code))
						.Distinct(StringComparer.OrdinalIgnoreCase)
						.ToArray();
				var overrides = await repository.GetScreeningRuleOverridesAsync(scoreModelId, normalizedCodes, cancellationToken);
				return new IndustryRuleSet(overrides);
		}
}