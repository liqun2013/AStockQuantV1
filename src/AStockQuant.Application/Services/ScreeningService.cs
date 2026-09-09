using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;

namespace AStockQuant.Application.Services;

public sealed class ScreeningService(IScreeningRepository repository) : IScreeningService
{
		public async Task<IReadOnlyList<StockCandidateDto>> GetCandidatesAsync(string profileCode, string version, DateOnly? scoreDate, CancellationToken cancellationToken = default)
		{
				var profile = await repository.GetProfileByCodeAndVersionAsync(profileCode, version, cancellationToken)
						?? throw new InvalidOperationException($"Screening profile '{profileCode}@{version}' not found or inactive.");
				var targetScoreDate = scoreDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
				var contexts = await repository.GetCandidateContextsAsync(profile.ScoreModelId, targetScoreDate, cancellationToken);

				return contexts
						.Select(profile.Decide)
						.Where(decision => decision.IsSelected)
						.OrderByDescending(decision => decision.Context.FinalScore)
						.ThenBy(decision => decision.Context.StockCode, StringComparer.Ordinal)
						.Take(profile.TopN)
						.Select(decision => new StockCandidateDto(
								decision.Context.StockCode,
								decision.Context.StockName,
								decision.Context.IndustryCode,
								decision.Context.IndustryName,
								decision.Context.ScoreDate,
								decision.Context.FinancialReportDate,
								decision.Context.BuffettScore,
								decision.Context.GrahamScore,
								decision.Context.FisherScore,
								decision.Context.FinalScore))
						.ToArray();
		}
}