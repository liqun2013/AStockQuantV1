using AStockQuant.Application.DTOs;

namespace AStockQuant.Application.Interfaces;

public interface IDataSyncService
{
		Task<SyncExecutionResult> SynchronizeAsync(SyncRequest request, CancellationToken cancellationToken = default);
}
