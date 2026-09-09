using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Application.Services;
using FluentAssertions;
using Xunit;

namespace AStockQuant.Application.Tests;

public sealed class StockAnalysisServiceTests
{
    [Fact]
    public async Task GetStocksAsync_ClampsInvalidPaging()
    {
        var repository = new InMemoryStockRepository();
        var service = new StockAnalysisService(repository, new InMemoryScoreModelRuleRepository());
        await service.GetStocksAsync(-1, 999);
        repository.LastPageIndex.Should().Be(1);
        repository.LastPageSize.Should().Be(200);
    }

    [Fact]
    public async Task CalculateAndSaveScoreAsync_SavesCalculatedScore_WhenSnapshotExists()
    {
        var repository = new InMemoryStockRepository();
        var service = new StockAnalysisService(repository, new InMemoryScoreModelRuleRepository());
        var score = await service.CalculateAndSaveScoreAsync("600519", new DateOnly(2026, 8, 26));
        score.Should().NotBeNull();
        repository.SavedScore.Should().Be(score);
    }

    [Fact]
    public async Task GetCandidatesAsync_NormalizesScreeningBounds()
    {
        var repository = new InMemoryStockRepository();
        var service = new StockAnalysisService(repository, new InMemoryScoreModelRuleRepository());
        var scoreDate = new DateOnly(2026, 8, 26);

        await service.GetCandidatesAsync("VALUE_QUALITY", "V1.0", scoreDate);

        repository.LastProfileCode.Should().Be("VALUE_QUALITY");
        repository.LastVersion.Should().Be("V1.0");
        repository.LastScoreDate.Should().Be(scoreDate);
    }

    private sealed class InMemoryStockRepository : IStockRepository
    {
        public int LastPageIndex { get; private set; }
        public int LastPageSize { get; private set; }
        public string? LastProfileCode { get; private set; }
        public string? LastVersion { get; private set; }
        public DateOnly? LastScoreDate { get; private set; }
        public InvestmentScoreDto? SavedScore { get; private set; }
        public Task<PagedResult<StockDto>> GetStocksAsync(int pageIndex, int pageSize, string? market, string? industry, CancellationToken cancellationToken) { LastPageIndex = pageIndex; LastPageSize = pageSize; return Task.FromResult(new PagedResult<StockDto>(0, [])); }
        public Task<StockDto?> GetStockAsync(string code, CancellationToken cancellationToken) => Task.FromResult<StockDto?>(new StockDto(code, "Kweichow Moutai", "SSE", null, true));
        public Task<IReadOnlyList<DailyPriceDto>> GetDailyPricesAsync(string code, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<DailyPriceDto>>([]);
        public Task<IReadOnlyList<InvestmentScoreDto>> GetRankingAsync(DateOnly scoreDate, decimal? minScore, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<InvestmentScoreDto>>([]);
        public Task<IReadOnlyList<StockCandidateDto>> GetCandidatesAsync(string profileCode, string version, DateOnly? scoreDate, CancellationToken cancellationToken) { LastProfileCode = profileCode; LastVersion = version; LastScoreDate = scoreDate; return Task.FromResult<IReadOnlyList<StockCandidateDto>>([]); }
        public Task<InvestmentScoreDto?> GetLatestScoreAsync(string code, CancellationToken cancellationToken) => Task.FromResult<InvestmentScoreDto?>(null);
        public Task<FinancialSnapshotDto?> GetFinancialSnapshotAsync(string code, DateOnly asOfDate, CancellationToken cancellationToken) => Task.FromResult<FinancialSnapshotDto?>(new FinancialSnapshotDto(code, asOfDate, 20m, 15m, 45m, 20m, 1m, 35m, 2m, 14m, 1.1m, 2m, 10m, 15m, 20m, 18m, 8m));
        public Task SaveInvestmentScoreAsync(InvestmentScoreDto score, string modelCode, CancellationToken cancellationToken) { SavedScore = score; return Task.CompletedTask; }
    }

    private sealed class InMemoryScoreModelRuleRepository : IScoreModelRuleRepository
    {
        public Task<IReadOnlyList<ScoreModelWeightDto>> GetModelWeightsAsync(string modelCode, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ScoreModelWeightDto>>(
            [
                new("BUFFETT", 0.40m),
                new("GRAHAM", 0.20m),
                new("FISHER", 0.40m)
            ]);
    }
}