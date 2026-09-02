using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Infrastructure.Persistence;
using Dapper;

namespace AStockQuant.Infrastructure.Repositories;

public sealed class MarketDataRepository(ISqlConnectionFactory connectionFactory) : IMarketDataRepository
{
		public async Task<SyncResult> UpsertStocksAsync(IReadOnlyCollection<StockImportDto> stocks, CancellationToken cancellationToken = default)
		{
				using var connection = connectionFactory.CreateConnection();
				connection.Open();
				using var transaction = connection.BeginTransaction();
				var succeeded = 0;
				try
				{
						const string sql = """
MERGE Basic.Exchange AS target
USING (SELECT @ExchangeCode AS ExchangeCode) AS source
ON target.ExchangeCode = source.ExchangeCode
WHEN NOT MATCHED THEN INSERT (ExchangeCode, ExchangeName, CreatedTime)
VALUES (source.ExchangeCode, source.ExchangeCode, SYSUTCDATETIME());

MERGE Basic.Stock AS target
USING
(
		SELECT
				@StockCode AS StockCode,
				@StockName AS StockName,
				(SELECT ExchangeId FROM Basic.Exchange WHERE ExchangeCode = @ExchangeCode) AS ExchangeId,
				@SecurityType AS SecurityType,
				@MarketType AS MarketType,
				@ListingDate AS ListingDate,
				@IsActive AS IsActive
) AS source
ON target.StockCode = source.StockCode
WHEN MATCHED THEN UPDATE SET
		StockName = source.StockName,
		ExchangeId = source.ExchangeId,
		SecurityType = source.SecurityType,
		MarketType = source.MarketType,
		ListingDate = COALESCE(source.ListingDate, target.ListingDate),
		IsActive = source.IsActive,
		UpdatedTime = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (StockCode, StockName, ExchangeId, SecurityType, MarketType, ListingDate, IsActive, CreatedTime, UpdatedTime)
VALUES (source.StockCode, source.StockName, source.ExchangeId, source.SecurityType, source.MarketType, source.ListingDate, source.IsActive, SYSUTCDATETIME(), SYSUTCDATETIME());
""";
						foreach (var stock in stocks)
						{
								await connection.ExecuteAsync(new CommandDefinition(sql, stock, transaction, cancellationToken: cancellationToken));
								succeeded++;
						}
						transaction.Commit();
						return new SyncResult("Stock", stocks.Count, succeeded, 0, stocks.Count - succeeded);
				}
				catch (Exception exception)
				{
						transaction.Rollback();
						return new SyncResult("Stock", stocks.Count, 0, 0, stocks.Count, exception.Message);
				}
		}

		public async Task<SyncResult> UpsertDailyPricesAsync(IReadOnlyCollection<DailyPriceImportDto> prices, CancellationToken cancellationToken = default)
		{
				using var connection = connectionFactory.CreateConnection();
				connection.Open();
				using var transaction = connection.BeginTransaction();
				var succeeded = 0;
				try
				{
						const string sql = """
MERGE Market.StockDailyPrice AS target
USING
(
		SELECT
				(SELECT StockId FROM Basic.Stock WHERE StockCode = @StockCode) AS StockId,
				@TradeDate AS TradeDate,
				@OpenPrice AS OpenPrice, @HighPrice AS HighPrice, @LowPrice AS LowPrice,
				@ClosePrice AS ClosePrice, @PrevClosePrice AS PrevClosePrice,
				@ChangeAmount AS ChangeAmount, @ChangePercent AS ChangePercent,
				@Volume AS Volume, @Amount AS Amount, @TurnoverRate AS TurnoverRate,
				@TotalMarketCap AS TotalMarketCap, @FloatMarketCap AS FloatMarketCap,
				@IsSuspended AS IsSuspended, @Source AS Source
) AS source
ON target.StockId = source.StockId AND target.TradeDate = source.TradeDate
WHEN MATCHED THEN UPDATE SET
		OpenPrice = source.OpenPrice, HighPrice = source.HighPrice, LowPrice = source.LowPrice,
		ClosePrice = source.ClosePrice, PrevClosePrice = source.PrevClosePrice,
		ChangeAmount = source.ChangeAmount, ChangePercent = source.ChangePercent,
		Volume = source.Volume, Amount = source.Amount, TurnoverRate = source.TurnoverRate,
		TotalMarketCap = source.TotalMarketCap, FloatMarketCap = source.FloatMarketCap,
		IsSuspended = source.IsSuspended, Source = source.Source
WHEN NOT MATCHED AND source.StockId IS NOT NULL THEN
		INSERT (StockId, TradeDate, OpenPrice, HighPrice, LowPrice, ClosePrice, PrevClosePrice, ChangeAmount, ChangePercent, Volume, Amount, TurnoverRate, TotalMarketCap, FloatMarketCap, IsSuspended, Source, CreatedTime)
		VALUES (source.StockId, source.TradeDate, source.OpenPrice, source.HighPrice, source.LowPrice, source.ClosePrice, source.PrevClosePrice, source.ChangeAmount, source.ChangePercent, source.Volume, source.Amount, source.TurnoverRate, source.TotalMarketCap, source.FloatMarketCap, source.IsSuspended, source.Source, SYSUTCDATETIME());
""";
						foreach (var price in prices)
						{
								await connection.ExecuteAsync(new CommandDefinition(sql, price, transaction, cancellationToken: cancellationToken));
								succeeded++;
						}
						transaction.Commit();
						return new SyncResult("DailyPrice", prices.Count, succeeded, 0, prices.Count - succeeded);
				}
				catch (Exception exception)
				{
						transaction.Rollback();
						return new SyncResult("DailyPrice", prices.Count, 0, 0, prices.Count, exception.Message);
				}
		}
}
