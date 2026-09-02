using AStockQuant.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AStockQuant.Worker.Jobs;

public sealed class MarketDataSyncJob(IServiceScopeFactory scopeFactory, ILogger<MarketDataSyncJob> logger) : BackgroundService
{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
				using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
				do
				{
						await RunOnceAsync(stoppingToken);
				} while (await timer.WaitForNextTickAsync(stoppingToken));
		}

		private async Task RunOnceAsync(CancellationToken cancellationToken)
		{
				await using var scope = scopeFactory.CreateAsyncScope();
				var provider = scope.ServiceProvider.GetRequiredService<IMarketDataProvider>();
				var repository = scope.ServiceProvider.GetRequiredService<IMarketDataRepository>();
				var stocks = await RetryAsync(() => provider.GetStocksAsync(cancellationToken), cancellationToken);
				var stockResult = await repository.UpsertStocksAsync(stocks, cancellationToken);
				logger.LogInformation("Stock synchronization completed: requested={Requested}, succeeded={Succeeded}, failed={Failed}.", stockResult.Requested, stockResult.Succeeded, stockResult.Failed);

				var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
				var startDate = endDate.AddDays(-30);
				var total = 0;
				foreach (var stock in stocks)
				{
						var prices = await RetryAsync(() => provider.GetDailyPricesAsync(stock.StockCode, startDate, endDate, cancellationToken), cancellationToken);
						var result = await repository.UpsertDailyPricesAsync(prices, cancellationToken);
						total += result.Succeeded;
				}
				logger.LogInformation("Daily price synchronization completed: succeeded={Succeeded}.", total);
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
