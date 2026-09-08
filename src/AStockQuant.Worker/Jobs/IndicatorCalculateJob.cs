using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AStockQuant.Worker.Jobs;

public sealed class IndicatorCalculateJob(IServiceScopeFactory scopeFactory, ISyncStageCoordinator coordinator, ILogger<IndicatorCalculateJob> logger) : BackgroundService
{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
				using var timer = new PeriodicTimer(TimeSpan.FromDays(7));
				do
				{
						await coordinator.WaitForCompletionAsync(SyncStage.FinancialData, stoppingToken);
						await RunOnceAsync(stoppingToken);
						coordinator.Complete(SyncStage.FinancialIndicator);
				} while (await timer.WaitForNextTickAsync(stoppingToken));
		}

		private async Task RunOnceAsync(CancellationToken cancellationToken)
		{
				await using var scope = scopeFactory.CreateAsyncScope();
				var stockRepository = scope.ServiceProvider.GetRequiredService<IStockRepository>();
				var indicatorRepository = scope.ServiceProvider.GetRequiredService<IFinancialIndicatorRepository>();
				var logRepository = scope.ServiceProvider.GetRequiredService<IImportLogRepository>();
				var handle = await logRepository.StartAsync("FinancialIndicator", cancellationToken: cancellationToken);
				var pageIndex = 1;
				var requested = 0;
				var succeeded = 0;
				var skipped = 0;
				var failed = 0;

				while (!cancellationToken.IsCancellationRequested)
				{
						var stocks = await stockRepository.GetStocksAsync(pageIndex, 200, null, null, cancellationToken);
						if (stocks.Items.Count == 0) break;

						var result = await indicatorRepository.CalculateAndUpsertAsync(stocks.Items.Select(stock => stock.Code).ToArray(), cancellationToken);
						requested += result.Requested;
						succeeded += result.Succeeded;
						skipped += result.Skipped;
						failed += result.Failed;

						if (pageIndex * 200 >= stocks.Total) break;
						pageIndex++;
				}

				var resultSummary = new SyncResult("FinancialIndicator", requested, succeeded, skipped, failed);
				await logRepository.CompleteAsync(handle, resultSummary, cancellationToken);
				if (resultSummary.Failed > 0) throw new InvalidOperationException($"Financial indicator calculation failed for {resultSummary.Failed} rows.");
				logger.LogInformation("Financial indicator calculation completed: succeeded={Succeeded}, failed={Failed}.", succeeded, failed);
		}
}
