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
				var priceRows = 0;
				var reportRows = 0;
				var indicatorRows = 0;

				foreach (var stock in selectedStocks)
				{
						cancellationToken.ThrowIfCancellationRequested();
						try
						{
								var prices = await marketDataProvider.GetDailyPricesAsync(stock.StockCode, startDate, endDate, cancellationToken);
								var priceResult = await RunLoggedAsync("DailyPrice", prices, values => marketDataRepository.UpsertDailyPricesAsync(values, cancellationToken), errors, cancellationToken);
								priceRows += priceResult.Succeeded;

								var reports = await financialDataProvider.GetReportsAsync(stock.StockCode, cancellationToken);
								var reportResult = await RunLoggedAsync("FinancialReport", reports, values => financialDataRepository.UpsertReportsAsync(values, cancellationToken), errors, cancellationToken);
								reportRows += reportResult.Succeeded;

								var indicatorResult = await RunLoggedAsync("FinancialIndicator", Array.Empty<StockImportDto>(), _ => financialIndicatorRepository.CalculateAndUpsertAsync(stock.StockCode, cancellationToken), errors, cancellationToken);
								indicatorRows += indicatorResult.Succeeded;
						}
						catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
						{
								throw;
						}
						catch (Exception exception)
						{
								errors.Add($"{stock.StockCode}: {exception.Message}");
						}
				}

				return new SyncExecutionResult(errors.Count == 0, selectedStocks.Length, priceRows, reportRows, indicatorRows, errors);
		}

		private async Task<SyncResult> RunLoggedAsync<T>(string dataType, IReadOnlyCollection<T> values, Func<IReadOnlyCollection<T>, Task<SyncResult>> action, List<string> errors, CancellationToken cancellationToken)
		{
				ImportLogHandle? handle = null;
				try
				{
						handle = await importLogRepository.StartAsync(dataType, cancellationToken: cancellationToken);
						var result = await action(values);
						await importLogRepository.CompleteAsync(handle, result, cancellationToken);
						if (result.Error is not null) errors.Add($"{dataType}: {result.Error}");
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
