using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Domain.Services;
using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Application.Services;

public sealed class StockAnalysisService(IStockRepository repository)
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
    public Task<InvestmentScoreDto?> GetLatestScoreAsync(string code, CancellationToken cancellationToken = default) => repository.GetLatestScoreAsync(code, cancellationToken);
    public Task<FinancialSnapshotDto?> GetFinancialSnapshotAsync(string code, DateOnly asOfDate, CancellationToken cancellationToken = default) => repository.GetFinancialSnapshotAsync(code, asOfDate, cancellationToken);

    public async Task<InvestmentScoreDto?> CalculateAndSaveScoreAsync(string code, DateOnly asOfDate, CancellationToken cancellationToken = default)
    {
        var snapshot = await repository.GetFinancialSnapshotAsync(code, asOfDate, cancellationToken);
        if (snapshot is null) return null;
        var score = calculator.Calculate(ToDomain(snapshot));
        var dto = new InvestmentScoreDto(score.StockCode, score.ScoreDate, score.BuffettScore, score.GrahamScore, score.FisherScore, score.FinalScore);
        await repository.SaveInvestmentScoreAsync(dto, cancellationToken);
        return dto;
    }

    private static FinancialSnapshot ToDomain(FinancialSnapshotDto d) => new(d.StockCode, d.AsOfDate, d.Roe, d.Roic, d.GrossMargin, d.NetMargin, d.OperatingCashFlowToNetProfit, d.DebtAssetRatio, d.CurrentRatio, d.Pe, d.Pb, d.Eps, d.Bvps, d.MarketPrice, d.RevenueGrowth3Y, d.ProfitGrowth3Y, d.ResearchExpenseRatio);
}