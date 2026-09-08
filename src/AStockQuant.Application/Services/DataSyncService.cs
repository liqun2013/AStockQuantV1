using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;

namespace AStockQuant.Application.Services;

public sealed class DataSyncService(
		IMarketDataProvider marketDataProvider,
		IMarketDataRepository marketDataRepository,
		IFinancialDataProvider financialDataProvider,
		IFinancialDataRepository financialDataRepository,
		IFinancialIndicatorRepository financialIndicatorRepository,
		IImportLogRepository importLogRepository) : IDataSyncService
{
		public async Task<SyncExecutionResult> SynchronizeAsync(SyncRequest request, CancellationToken cancellationToken = default)
		{
				var errors = new List<string>();
				var endDate = request.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
				var startDate = request.StartDate ?? endDate.AddDays(-30);
				if (startDate > endDate) throw new ArgumentException("StartDate must not be later than EndDate.", nameof(request));

				var stocks = await marketDataProvider.GetStocksAsync(cancellationToken);
				var selectedStocks = string.IsNullOrWhiteSpace(request.StockCode)
						? stocks.Take(Math.Clamp(request.MaxStocks, 1, 200)).ToArray()
						: stocks.Where(stock => stock.StockCode.Equals(request.StockCode.Trim(), StringComparison.OrdinalIgnoreCase)).ToArray();
				if (selectedStocks.Length == 0)
						return new SyncExecutionResult(false, 0, 0, 0, 0, [$"Stock '{request.StockCode}' was not found in AKTools."]);

				var stockResult = await RunLoggedAsync("Stock", selectedStocks, values => marketDataRepository.UpsertStocksAsync(values, cancellationToken), errors, cancellationToken);
				var stockCodes = selectedStocks.Select(stock => stock.StockCode).ToArray();
				var prices = await marketDataProvider.GetDailyPricesAsync(stockCodes, startDate, endDate, cancellationToken);
				var priceResult = await RunLoggedAsync("DailyPrice", prices, values => marketDataRepository.UpsertDailyPricesAsync(values, cancellationToken), errors, cancellationToken);

				var reports = await financialDataProvider.GetReportsAsync(stockCodes, cancellationToken);
				var reportResult = await RunLoggedAsync("FinancialReport", reports, values => financialDataRepository.UpsertReportsAsync(values, cancellationToken), errors, cancellationToken);

				var indicatorResult = await RunLoggedAsync("FinancialIndicator", stockCodes, values => financialIndicatorRepository.CalculateAndUpsertAsync(values, cancellationToken), errors, cancellationToken);

				return new SyncExecutionResult(errors.Count == 0, selectedStocks.Length, priceResult.Succeeded, reportResult.Succeeded, indicatorResult.Succeeded, errors);
		}

		private async Task<SyncResult> RunLoggedAsync<T>(string dataType, IReadOnlyCollection<T> values, Func<IReadOnlyCollection<T>, Task<SyncResult>> action, List<string> errors, CancellationToken cancellationToken)
		{
				ImportLogHandle? handle = null;
				try
				{
						handle = await importLogRepository.StartAsync(dataType, cancellationToken: cancellationToken);
						var result = await action(values);
						await importLogRepository.CompleteAsync(handle, result, cancellationToken);
						if (result.Failed > 0 && result.Error is not null) errors.Add($"{dataType}: {result.Error}");
						return result;
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
						throw;
				}
				catch (Exception exception)
				{
						errors.Add($"{dataType}: {exception.Message}");
						if (handle is not null)
						{
								try
								{
										await importLogRepository.CompleteAsync(handle, new SyncResult(dataType, values.Count, 0, 0, values.Count, exception.Message), cancellationToken);
								}
								catch (Exception logException)
								{
										errors.Add($"{dataType} log: {logException.Message}");
								}
						}
						return new SyncResult(dataType, values.Count, 0, 0, values.Count, exception.Message);
				}
		}
}
