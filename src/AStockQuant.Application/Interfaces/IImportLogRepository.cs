using AStockQuant.Application.DTOs;

namespace AStockQuant.Application.Interfaces;

public interface IImportLogRepository
{
		Task<ImportLogHandle> StartAsync(string dataType, int? dataSourceId = null, CancellationToken cancellationToken = default);
		Task CompleteAsync(ImportLogHandle handle, SyncResult result, CancellationToken cancellationToken = default);
}

public sealed record ImportLogHandle(long ImportLogId, string CorrelationId);
