using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Domain.Services;
using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Application.Services;

public sealed class StockAnalysisService(IStockRepository repository, IScoreModelRuleRepository ruleRepository)
{
    private readonly CompositeScoreCalculator calculator = new();

    public Task<PagedResult<StockDto>> GetStocksAsync(int pageIndex, int pageSize, string? market = null, string? industry = null, CancellationToken cancellationToken = default)
    {
        pageIndex = Math.Max(1, pageIndex);
        pageSize = Math.Clamp(pageSize, 1, 200);
        return repository.GetStocksAsync(pageIndex, pageSize, market, industry, cancellationToken);
    }

    public Task<StockDto?> GetStockAsync(string code, CancellationToken cancellationToken = default) => repository.GetStockAsync(code, cancellationToken);
    public Task<IReadOnlyList<DailyPriceDto>> GetDailyPricesAsync(string code, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken = default) => repository.GetDailyPricesAsync(code, startDate, endDate, cancellationToken);
    public Task<IReadOnlyList<InvestmentScoreDto>> GetRankingAsync(DateOnly scoreDate, decimal? minScore, CancellationToken cancellationToken = default) => repository.GetRankingAsync(scoreDate, minScore, cancellationToken);
    public Task<IReadOnlyList<StockCandidateDto>> GetCandidatesAsync(string profileCode, string version, DateOnly? scoreDate, CancellationToken cancellationToken = default)
    {
        var normalizedScoreDate = scoreDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return repository.GetCandidatesAsync(profileCode, version, normalizedScoreDate, cancellationToken);
    }
    public Task<InvestmentScoreDto?> GetLatestScoreAsync(string code, CancellationToken cancellationToken = default) => repository.GetLatestScoreAsync(code, cancellationToken);
    public Task<FinancialSnapshotDto?> GetFinancialSnapshotAsync(string code, DateOnly asOfDate, CancellationToken cancellationToken = default) => repository.GetFinancialSnapshotAsync(code, asOfDate, cancellationToken);

    public async Task<InvestmentScoreDto?> CalculateAndSaveScoreAsync(string code, DateOnly asOfDate, CancellationToken cancellationToken = default)
    {
        var snapshot = await repository.GetFinancialSnapshotAsync(code, asOfDate, cancellationToken);
        if (snapshot is null) return null;
        var weights = await GetWeightsAsync(cancellationToken);
        var score = calculator.Calculate(ToDomain(snapshot), weights);
        var dto = new InvestmentScoreDto(score.StockCode, score.ScoreDate, score.BuffettScore, score.GrahamScore, score.FisherScore, score.FinalScore);
        await repository.SaveInvestmentScoreAsync(dto, ModelCode, cancellationToken);
        return dto;
    }

    private static FinancialSnapshot ToDomain(FinancialSnapshotDto d) => new(d.StockCode, d.AsOfDate, d.Roe, d.Roic, d.GrossMargin, d.NetMargin, d.OperatingCashFlowToNetProfit, d.DebtAssetRatio, d.CurrentRatio, d.Pe, d.Pb, d.Eps, d.Bvps, d.MarketPrice, d.RevenueGrowth3Y, d.ProfitGrowth3Y, d.ResearchExpenseRatio);

    private const string ModelCode = "VALUE_INVESTMENT";

    private async Task<CompositeScoreWeights> GetWeightsAsync(CancellationToken cancellationToken)
    {
        var configuredWeights = await ruleRepository.GetModelWeightsAsync(ModelCode, cancellationToken);
        var weights = configuredWeights
            .GroupBy(item => item.ComponentCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Single().Weight, StringComparer.OrdinalIgnoreCase);

        if (!weights.TryGetValue("BUFFETT", out var buffett) ||
            !weights.TryGetValue("GRAHAM", out var graham) ||
            !weights.TryGetValue("FISHER", out var fisher) ||
            weights.Count != 3)
        {
            throw new InvalidOperationException($"Active score model '{ModelCode}' must define exactly BUFFETT, GRAHAM and FISHER weights.");
        }

        return new CompositeScoreWeights(buffett, graham, fisher);
    }
}