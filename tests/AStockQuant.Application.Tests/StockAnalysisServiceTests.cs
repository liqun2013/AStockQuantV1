using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Application.Services;
using AStockQuant.Domain.ValueObjects;
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
    public async Task CalculateAndSaveScoreAsync_UsesDatabaseRulesForEveryInvestmentModel()
    {
        var repository = new InMemoryStockRepository();
        var ruleRepository = new InMemoryScoreModelRuleRepository();
        var service = new StockAnalysisService(repository, ruleRepository);
        var score = await service.CalculateAndSaveScoreAsync("600519", new DateOnly(2026, 8, 26));

        score.Should().NotBeNull();
        repository.SavedScore.Should().Be(score);
        score!.BuffettScore.Should().Be(82.9412m);
        score.GrahamScore.Should().Be(76m);
        score.FisherScore.Should().Be(80m);
        score.FinalScore.Should().Be(80.3765m);
        repository.SavedExplanation.Select(model => model.ModelCode).Should().Equal("BUFFETT", "GRAHAM", "FISHER");
        repository.SavedExplanation.SelectMany(model => model.Components).Should().NotBeEmpty();
        repository.SavedExplanation.SelectMany(model => model.Components).Select(component => component.ComponentCode)
            .Should().Contain(new[] { "B01", "G01", "F01" });
        ruleRepository.LoadedModels.Should().Equal("BUFFETT", "GRAHAM", "FISHER");
    }

    private sealed class InMemoryStockRepository : IStockRepository
    {
        public int LastPageIndex { get; private set; }
        public int LastPageSize { get; private set; }
        public InvestmentScoreDto? SavedScore { get; private set; }
        public IReadOnlyList<ScoreModelExplanationDto> SavedExplanation { get; private set; } = [];
        public Task<PagedResult<StockDto>> GetStocksAsync(int pageIndex, int pageSize, string? market, string? industry, CancellationToken cancellationToken) { LastPageIndex = pageIndex; LastPageSize = pageSize; return Task.FromResult(new PagedResult<StockDto>(0, [])); }
        public Task<StockDto?> GetStockAsync(string code, CancellationToken cancellationToken) => Task.FromResult<StockDto?>(new StockDto(code, "Kweichow Moutai", "SSE", null, true));
        public Task<IReadOnlyList<DailyPriceDto>> GetDailyPricesAsync(string code, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<DailyPriceDto>>([]);
        public Task<IReadOnlyList<InvestmentScoreDto>> GetRankingAsync(DateOnly scoreDate, decimal? minScore, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<InvestmentScoreDto>>([]);
        public Task<InvestmentScoreDto?> GetLatestScoreAsync(string code, CancellationToken cancellationToken) => Task.FromResult<InvestmentScoreDto?>(null);
        public Task<ScoreExplanationDto?> GetLatestScoreExplanationAsync(string code, CancellationToken cancellationToken) => Task.FromResult<ScoreExplanationDto?>(null);
        public Task<FinancialSnapshotDto?> GetFinancialSnapshotAsync(string code, DateOnly asOfDate, CancellationToken cancellationToken) => Task.FromResult<FinancialSnapshotDto?>(new FinancialSnapshotDto(
            code, asOfDate, 20m, 15m, 45m, 20m, 1m, 35m, 2m, 14m, 1.7m, 2m, 10m, 15m, 25m, 22m, 15m, 20m, 0.8m, 3.5m));
        public Task SaveInvestmentScoreAsync(InvestmentScoreDto score, string modelCode, IReadOnlyList<ScoreModelExplanationDto> explanation, CancellationToken cancellationToken) { SavedScore = score; SavedExplanation = explanation; return Task.CompletedTask; }
    }

    private sealed class InMemoryScoreModelRuleRepository : IScoreModelRuleRepository
    {
        public List<string> LoadedModels { get; } = [];

        public Task<IReadOnlyList<ScoreModelWeightDto>> GetModelWeightsAsync(string modelCode, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ScoreModelWeightDto>>(
            [
                new("BUFFETT", 0.40m),
                new("GRAHAM", 0.20m),
                new("FISHER", 0.40m)
            ]);

        public Task<IReadOnlyList<InvestmentIndicatorRule>> GetIndicatorRulesAsync(string modelCode, CancellationToken cancellationToken)
        {
            LoadedModels.Add(modelCode);
            IReadOnlyList<InvestmentIndicatorRule> rules = modelCode switch
            {
                "BUFFETT" =>
                [
                    Rule("B01", 25m, 20m, 25m, 9m),
                    Rule("B02", 25m, 15m, 20m, 8m),
                    Rule("B03", 20m, 0.8m, 1m, 8m),
                    Rule("B04", 15m, 30m, 50m, 8m)
                ],
                "GRAHAM" =>
                [
                    Rule("G01", 30m, 10m, 15m, 8m),
                    Rule("G02", 20m, 1.5m, 2m, 8m),
                    Rule("G03", 20m, 3m, 4m, 6m),
                    Rule("G04", 15m, 30m, 50m, 8m),
                    Rule("G05", 15m, 0.8m, 1m, 8m)
                ],
                "FISHER" =>
                [
                    Rule("F01", 20m, 20m, 30m, 8m),
                    Rule("F02", 25m, 20m, 30m, 8m),
                    Rule("F03", 25m, 15m, 25m, 8m)
                ],
                _ => throw new ArgumentOutOfRangeException(nameof(modelCode))
            };

            return Task.FromResult(rules);
        }

        private static InvestmentIndicatorRule Rule(string code, decimal weight, decimal min, decimal max, decimal score) =>
            new(code, weight, 10m, min, max, score, 1);
    }
}