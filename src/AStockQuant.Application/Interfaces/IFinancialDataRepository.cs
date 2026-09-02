using AStockQuant.Application.DTOs;

namespace AStockQuant.Application.Interfaces;

public interface IFinancialDataRepository
{
		Task<SyncResult> UpsertReportsAsync(IReadOnlyCollection<FinancialReportImportDto> reports, CancellationToken cancellationToken = default);
}
