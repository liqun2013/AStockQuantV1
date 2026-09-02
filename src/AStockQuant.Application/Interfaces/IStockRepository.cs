using AStockQuant.Application.DTOs;
namespace AStockQuant.Application.Interfaces;
public interface IStockRepository
{
    Task<PagedResult<StockDto>> GetStocksAsync(int pageIndex, int pageSize, string? market, string? industry, CancellationToken cancellationToken);
    Task<StockDto?> GetStockAsync(string code, CancellationToken cancellationToken);
    Task<IReadOnlyList<DailyPriceDto>> GetDailyPricesAsync(string code, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken);
    Task<IReadOnlyList<InvestmentScoreDto>> GetRankingAsync(DateOnly scoreDate, decimal? minScore, CancellationToken cancellationToken);
    Task<InvestmentScoreDto?> GetLatestScoreAsync(string code, CancellationToken cancellationToken);
    Task<FinancialSnapshotDto?> GetFinancialSnapshotAsync(string code, DateOnly asOfDate, CancellationToken cancellationToken);
    Task SaveInvestmentScoreAsync(InvestmentScoreDto score, CancellationToken cancellationToken);
}