using AStockQuant.Application.DTOs;

namespace AStockQuant.Application.Interfaces;

public interface IFinancialIndicatorRepository
{
		Task<SyncResult> CalculateAndUpsertAsync(IReadOnlyCollection<string> stockCodes, CancellationToken cancellationToken = default);
}
