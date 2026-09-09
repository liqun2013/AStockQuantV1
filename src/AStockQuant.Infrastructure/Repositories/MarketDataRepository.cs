using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Infrastructure.Persistence;
using Dapper;
using System.Text.Json;

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
USING
(
		SELECT DISTINCT ExchangeCode
		FROM OPENJSON(@StocksJson) WITH (ExchangeCode VARCHAR(20) '$.ExchangeCode')
) AS source
ON target.ExchangeCode = source.ExchangeCode
WHEN NOT MATCHED THEN INSERT (ExchangeCode, ExchangeName, CreatedTime)
VALUES (source.ExchangeCode, source.ExchangeCode, SYSUTCDATETIME());

MERGE Basic.Stock AS target
USING
(
		SELECT
					source.StockCode, source.StockName, exchange.ExchangeId, source.SecurityType,
					source.MarketType, source.ListingDate, source.IsActive, source.IsST
		FROM OPENJSON(@StocksJson) WITH
		(
			StockCode VARCHAR(20) '$.StockCode', StockName NVARCHAR(100) '$.StockName',
			ExchangeCode VARCHAR(20) '$.ExchangeCode', SecurityType VARCHAR(50) '$.SecurityType',
			MarketType VARCHAR(50) '$.MarketType', ListingDate DATE '$.ListingDate', IsActive BIT '$.IsActive', IsST BIT '$.IsST'
		) AS source
		INNER JOIN Basic.Exchange exchange ON exchange.ExchangeCode = source.ExchangeCode
) AS source
ON target.StockCode = source.StockCode
WHEN MATCHED THEN UPDATE SET
		StockName = source.StockName,
		ExchangeId = source.ExchangeId,
		SecurityType = source.SecurityType,
		MarketType = source.MarketType,
		ListingDate = COALESCE(source.ListingDate, target.ListingDate),
		IsActive = source.IsActive,
		IsST = source.IsST,
		UpdatedTime = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (StockCode, StockName, ExchangeId, SecurityType, MarketType, ListingDate, IsActive, IsST, CreatedTime, UpdatedTime)
VALUES (source.StockCode, source.StockName, source.ExchangeId, source.SecurityType, source.MarketType, source.ListingDate, source.IsActive, source.IsST, SYSUTCDATETIME(), SYSUTCDATETIME());
""";
						await connection.ExecuteAsync(new CommandDefinition(sql, new { StocksJson = JsonSerializer.Serialize(stocks) }, transaction, cancellationToken: cancellationToken));
						succeeded = stocks.Count;
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
				stock.StockId, source.TradeDate, source.OpenPrice, source.HighPrice, source.LowPrice,
				source.ClosePrice, source.PrevClosePrice, source.ChangeAmount, source.ChangePercent,
				source.Volume, source.Amount, source.TurnoverRate, source.TotalMarketCap, source.FloatMarketCap,
				source.IsSuspended, source.Source
		FROM OPENJSON(@PricesJson) WITH
		(
			StockCode VARCHAR(20) '$.StockCode', TradeDate DATE '$.TradeDate', OpenPrice DECIMAL(24,8) '$.OpenPrice', HighPrice DECIMAL(24,8) '$.HighPrice', LowPrice DECIMAL(24,8) '$.LowPrice', ClosePrice DECIMAL(24,8) '$.ClosePrice', PrevClosePrice DECIMAL(24,8) '$.PrevClosePrice', ChangeAmount DECIMAL(24,8) '$.ChangeAmount', ChangePercent DECIMAL(24,8) '$.ChangePercent', Volume BIGINT '$.Volume', Amount DECIMAL(24,8) '$.Amount', TurnoverRate DECIMAL(24,8) '$.TurnoverRate', TotalMarketCap DECIMAL(24,8) '$.TotalMarketCap', FloatMarketCap DECIMAL(24,8) '$.FloatMarketCap', IsSuspended BIT '$.IsSuspended', Source VARCHAR(50) '$.Source'
		) AS source
		INNER JOIN Basic.Stock stock ON stock.StockCode = source.StockCode
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
						await connection.ExecuteAsync(new CommandDefinition(sql, new { PricesJson = JsonSerializer.Serialize(prices) }, transaction, cancellationToken: cancellationToken));
						succeeded = prices.Count;
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
