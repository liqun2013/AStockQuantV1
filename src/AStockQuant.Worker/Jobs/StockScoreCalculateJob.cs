using AStockQuant.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AStockQuant.Worker.Jobs;

public sealed class StockScoreCalculateJob(IServiceScopeFactory scopeFactory, ILogger<StockScoreCalculateJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        do
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<StockAnalysisService>();
            var scoreDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var pageIndex = 1;
            var processed = 0;
            logger.LogInformation("Stock score job started for {ScoreDate}.", scoreDate);

            while (!stoppingToken.IsCancellationRequested)
            {
                var stocks = await service.GetStocksAsync(pageIndex, 200, cancellationToken: stoppingToken);
                if (stocks.Items.Count == 0) break;

                foreach (var stock in stocks.Items)
                {
                    try
                    {
                        if (await service.CalculateAndSaveScoreAsync(stock.Code, scoreDate, stoppingToken) is not null)
                            processed++;
                    }
                    catch (Exception exception) when (exception is not OperationCanceledException)
                    {
                        logger.LogError(exception, "Failed to calculate score for stock {StockCode}.", stock.Code);
                    }
                }

                if (pageIndex * 200 >= stocks.Total) break;
                pageIndex++;
            }

            logger.LogInformation("Stock score job completed for {ScoreDate}. Processed {ProcessedCount} stocks.", scoreDate, processed);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
