using AStockQuant.Application.DTOs;

namespace AStockQuant.Application.Interfaces;

public interface IMarketDataProvider
{
		Task<IReadOnlyList<StockImportDto>> GetStocksAsync(CancellationToken cancellationToken = default);
		Task<IReadOnlyList<DailyPriceImportDto>> GetDailyPricesAsync(string stockCode, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<DailyPriceImportDto>> GetDailyPricesAsync(IReadOnlyCollection<string> stockCodes, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
}

public interface IFinancialDataProvider
{
		Task<IReadOnlyList<FinancialReportImportDto>> GetReportsAsync(string stockCode, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<FinancialReportImportDto>> GetReportsAsync(IReadOnlyCollection<string> stockCodes, CancellationToken cancellationToken = default);
}
