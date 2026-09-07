using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AStockQuant.Worker.Jobs;

public sealed class DataSyncHostedJob(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<DataSyncHostedJob> logger) : BackgroundService
{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
				var enabled = configuration.GetValue("Sync:Enabled", false);
				if (!enabled)
				{
						logger.LogInformation("Scheduled data synchronization is disabled.");
						return;
				}

				var intervalHours = Math.Clamp(configuration.GetValue("Sync:IntervalHours", 24), 1, 168);
				var timer = new PeriodicTimer(TimeSpan.FromHours(intervalHours));
				try
				{
						do
						{
							try
							{
									await RunOnceAsync(stoppingToken);
							}
							catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
							{
									logger.LogInformation("Scheduled data synchronization was cancelled.");
							}
							catch (Exception exception)
							{
									logger.LogError(exception, "Scheduled data synchronization failed.");
							}
						} while (await timer.WaitForNextTickAsync(stoppingToken));
				}
				finally
				{
						timer.Dispose();
				}
		}

		private async Task RunOnceAsync(CancellationToken cancellationToken)
		{
				var maxStocks = Math.Clamp(configuration.GetValue("Sync:MaxStocks", 1), 1, 20);
				var historyDays = Math.Clamp(configuration.GetValue("Sync:HistoryDays", 30), 1, 3650);
				var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
				var request = new SyncRequest(null, maxStocks, endDate.AddDays(-historyDays), endDate);
				await using var scope = scopeFactory.CreateAsyncScope();
				var service = scope.ServiceProvider.GetRequiredService<IDataSyncService>();
			var result = await RetryAsync(
					() => service.SynchronizeAsync(request, cancellationToken),
					configuration.GetValue("Sync:RetryCount", 3),
					cancellationToken);
				logger.LogInformation("Scheduled synchronization completed: succeeded={Succeeded}, stocks={Stocks}, prices={Prices}, reports={Reports}, indicators={Indicators}, errors={Errors}.", result.Succeeded, result.StocksProcessed, result.PriceRowsProcessed, result.FinancialReportsProcessed, result.IndicatorRowsProcessed, result.Errors.Count);
		}

		private static async Task<SyncExecutionResult> RetryAsync(Func<Task<SyncExecutionResult>> action, int retryCount, CancellationToken cancellationToken)
		{
				var attempts = Math.Max(1, retryCount + 1);
				for (var attempt = 1; ; attempt++)
				{
						try { return await action(); }
						catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
						catch when (attempt < attempts)
						{
								await Task.Delay(TimeSpan.FromSeconds(Math.Min(attempt * 2, 30)), cancellationToken);
						}
				}
		}
}
