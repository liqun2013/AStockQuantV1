using AStockQuant.Application.Interfaces;
using AStockQuant.Application.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AStockQuant.Worker.Jobs;

public sealed class FinancialDataSyncJob(IServiceScopeFactory scopeFactory, ILogger<FinancialDataSyncJob> logger) : BackgroundService
{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
				using var timer = new PeriodicTimer(TimeSpan.FromDays(7));
				do
				{
						await RunOnceAsync(stoppingToken);
				} while (await timer.WaitForNextTickAsync(stoppingToken));
		}

		private async Task RunOnceAsync(CancellationToken cancellationToken)
		{
				await using var scope = scopeFactory.CreateAsyncScope();
				var marketRepository = scope.ServiceProvider.GetRequiredService<IStockRepository>();
				var provider = scope.ServiceProvider.GetRequiredService<IFinancialDataProvider>();
				var repository = scope.ServiceProvider.GetRequiredService<IFinancialDataRepository>();
				var indicatorRepository = scope.ServiceProvider.GetRequiredService<IFinancialIndicatorRepository>();
				var logRepository = scope.ServiceProvider.GetRequiredService<IImportLogRepository>();
				var reportLog = await logRepository.StartAsync("FinancialReport", cancellationToken: cancellationToken);
				var pageIndex = 1;
				var processed = 0;
				while (!cancellationToken.IsCancellationRequested)
				{
						var stocks = await marketRepository.GetStocksAsync(pageIndex, 100, null, null, cancellationToken);
						if (stocks.Items.Count == 0) break;
						foreach (var stock in stocks.Items)
						{
								var reports = await RetryAsync(() => provider.GetReportsAsync(stock.Code, cancellationToken), cancellationToken);
								var result = await repository.UpsertReportsAsync(reports, cancellationToken);
								processed += result.Succeeded;
						}
						if (pageIndex * 100 >= stocks.Total) break;
						pageIndex++;
				}
				await logRepository.CompleteAsync(reportLog, new SyncResult("FinancialReport", processed, processed, 0, 0), cancellationToken);
				var indicatorLog = await logRepository.StartAsync("FinancialIndicator", cancellationToken: cancellationToken);
				var indicatorResult = await indicatorRepository.CalculateAndUpsertAsync(cancellationToken: cancellationToken);
				await logRepository.CompleteAsync(indicatorLog, indicatorResult, cancellationToken);
				logger.LogInformation("Financial synchronization completed: reports={Processed}.", processed);
		}

		private static async Task<T> RetryAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
		{
				for (var attempt = 1; ; attempt++)
				{
						try { return await action(); }
						catch when (attempt < 3)
						{
								await Task.Delay(TimeSpan.FromSeconds(attempt * 2), cancellationToken);
						}
				}
		}
}
