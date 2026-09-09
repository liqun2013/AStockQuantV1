using AStockQuant.Application.Interfaces;
using AStockQuant.Application.Services;
using AStockQuant.Domain.Screening;
using FluentAssertions;
using Xunit;

namespace AStockQuant.Application.Tests;

public sealed class ScreeningServiceTests
{
		[Fact]
		public async Task GetCandidatesAsync_ReturnsOnlySelectedStocksInScoreOrder()
		{
				var service = new ScreeningService(new FakeScreeningRepository());

				var candidates = await service.GetCandidatesAsync("VALUE_QUALITY", "V1.0", new DateOnly(2026, 8, 26));

				candidates.Select(candidate => candidate.StockCode).Should().Equal("000001", "600519");
		}

		private sealed class FakeScreeningRepository : IScreeningRepository
		{
				public Task<ScreeningProfile?> GetProfileByCodeAndVersionAsync(string profileCode, string version, CancellationToken cancellationToken) => Task.FromResult<ScreeningProfile?>(new ScreeningProfile(1, profileCode, "Test", version, 1, true,
				[
						new("TOP_N", 2m, null, null), new("MINIMUM_LISTING_YEARS", 3m, null, null), new("MINIMUM_FISHER_SCORE", 60m, null, null),
						new("MINIMUM_BUFFETT_SCORE", 60m, null, null), new("MINIMUM_GRAHAM_SCORE", 50m, null, null),
						new("REQUIRE_COMPLETE_FINANCIAL_DATA", null, true, null), new("EXCLUDE_ST", null, true, null)
				]));

				public Task<IReadOnlyList<ScreeningContext>> GetCandidateContextsAsync(int scoreModelId, DateOnly scoreDate, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ScreeningContext>>(
				[
						Create("600519", 75m, false), Create("000001", 90m, false), Create("000002", 95m, true)
				]);

				private static ScreeningContext Create(string code, decimal score, bool isSt) => new(1, code, code, null, null, new DateOnly(2026, 8, 26), new DateOnly(2020, 1, 1), new DateOnly(2025, 12, 31), 70m, 60m, 80m, score, true, isSt, true);
		}
}