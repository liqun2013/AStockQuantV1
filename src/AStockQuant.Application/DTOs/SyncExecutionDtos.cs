namespace AStockQuant.Application.DTOs;

public sealed record SyncRequest(
		string? StockCode = null,
		int MaxStocks = 1,
		DateOnly? StartDate = null,
		DateOnly? EndDate = null);

public sealed record SyncExecutionResult(
		bool Succeeded,
		int StocksProcessed,
		int PriceRowsProcessed,
		int FinancialReportsProcessed,
		int IndicatorRowsProcessed,
		IReadOnlyList<string> Errors);
