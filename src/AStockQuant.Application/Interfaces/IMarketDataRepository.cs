using AStockQuant.Application.DTOs;

namespace AStockQuant.Application.Interfaces;

public interface IMarketDataRepository
{
		Task<SyncResult> UpsertStocksAsync(IReadOnlyCollection<StockImportDto> stocks, CancellationToken cancellationToken = default);
		Task<SyncResult> UpsertDailyPricesAsync(IReadOnlyCollection<DailyPriceImportDto> prices, CancellationToken cancellationToken = default);
}
