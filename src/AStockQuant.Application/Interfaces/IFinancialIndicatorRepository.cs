using AStockQuant.Application.DTOs;

namespace AStockQuant.Application.Interfaces;

public interface IFinancialIndicatorRepository
{
		Task<SyncResult> CalculateAndUpsertAsync(string? stockCode = null, CancellationToken cancellationToken = default);
}
