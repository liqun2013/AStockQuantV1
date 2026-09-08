using AStockQuant.Application.Interfaces;
using AStockQuant.Application.DTOs;
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
				var logRepository = scope.ServiceProvider.GetRequiredService<IImportLogRepository>();
				var stocks = await RetryAsync(() => provider.GetStocksAsync(cancellationToken), cancellationToken);
				var stockLog = await logRepository.StartAsync("Stock", cancellationToken: cancellationToken);
				var stockResult = await repository.UpsertStocksAsync(stocks, cancellationToken);
				await logRepository.CompleteAsync(stockLog, stockResult, cancellationToken);
				logger.LogInformation("Stock synchronization completed: requested={Requested}, succeeded={Succeeded}, failed={Failed}.", stockResult.Requested, stockResult.Succeeded, stockResult.Failed);

				var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
				var startDate = endDate.AddDays(-30);
				var priceLog = await logRepository.StartAsync("DailyPrice", cancellationToken: cancellationToken);
				var prices = await RetryAsync(() => provider.GetDailyPricesAsync(stocks.Select(stock => stock.StockCode).ToArray(), startDate, endDate, cancellationToken), cancellationToken);
				var priceResult = await repository.UpsertDailyPricesAsync(prices, cancellationToken);
				await logRepository.CompleteAsync(priceLog, priceResult, cancellationToken);
				logger.LogInformation("Daily price synchronization completed: succeeded={Succeeded}.", priceResult.Succeeded);
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
