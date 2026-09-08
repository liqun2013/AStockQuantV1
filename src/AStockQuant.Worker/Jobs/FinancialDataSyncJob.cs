using AStockQuant.Application.Interfaces;
using AStockQuant.Application.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AStockQuant.Worker.Jobs;

public sealed class FinancialDataSyncJob(IServiceScopeFactory scopeFactory, ISyncStageCoordinator coordinator, ILogger<FinancialDataSyncJob> logger) : BackgroundService
{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
				using var timer = new PeriodicTimer(TimeSpan.FromDays(7));
				do
				{
						await coordinator.WaitForCompletionAsync(SyncStage.MarketData, stoppingToken);
						await RunOnceAsync(stoppingToken);
						coordinator.Complete(SyncStage.FinancialData);
				} while (await timer.WaitForNextTickAsync(stoppingToken));
		}

		private async Task RunOnceAsync(CancellationToken cancellationToken)
		{
				await using var scope = scopeFactory.CreateAsyncScope();
				var marketRepository = scope.ServiceProvider.GetRequiredService<IStockRepository>();
				var provider = scope.ServiceProvider.GetRequiredService<IFinancialDataProvider>();
				var repository = scope.ServiceProvider.GetRequiredService<IFinancialDataRepository>();
				var logRepository = scope.ServiceProvider.GetRequiredService<IImportLogRepository>();
				var reportLog = await logRepository.StartAsync("FinancialReport", cancellationToken: cancellationToken);
				var pageIndex = 1;
				var processed = 0;
				var failed = 0;
				while (!cancellationToken.IsCancellationRequested)
				{
						var stocks = await marketRepository.GetStocksAsync(pageIndex, 100, null, null, cancellationToken);
						if (stocks.Items.Count == 0) break;
						var reports = await RetryAsync(() => provider.GetReportsAsync(stocks.Items.Select(stock => stock.Code).ToArray(), cancellationToken), cancellationToken);
						var result = await repository.UpsertReportsAsync(reports, cancellationToken);
						processed += result.Succeeded;
						failed += result.Failed;
						if (pageIndex * 100 >= stocks.Total) break;
						pageIndex++;
				}
				var summary = new SyncResult("FinancialReport", processed + failed, processed, 0, failed);
				await logRepository.CompleteAsync(reportLog, summary, cancellationToken);
				if (summary.Failed > 0) throw new InvalidOperationException($"Financial report synchronization failed for {summary.Failed} rows.");
				logger.LogInformation("Financial report synchronization completed: reports={Processed}.", processed);
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
