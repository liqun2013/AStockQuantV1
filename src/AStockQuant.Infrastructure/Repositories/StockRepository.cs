using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Infrastructure.Persistence;
using Dapper;

namespace AStockQuant.Infrastructure.Repositories;

public sealed class StockRepository(ISqlConnectionFactory connectionFactory) : IStockRepository
{
    public async Task<PagedResult<StockDto>> GetStocksAsync(int pageIndex, int pageSize, string? market, string? industry, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        const string filter = """
FROM Basic.Stock s
INNER JOIN Basic.Exchange e ON e.ExchangeId = s.ExchangeId
WHERE (@Market IS NULL OR s.MarketType = @Market)
  AND (@Industry IS NULL OR EXISTS
  (
      SELECT 1
      FROM Basic.StockIndustry si
      INNER JOIN Basic.Industry i ON i.IndustryId = si.IndustryId
      WHERE si.StockId = s.StockId
        AND (i.IndustryCode = @Industry OR i.IndustryName = @Industry)
  ))
""";
        var parameters = new { Market = market, Industry = industry };
        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition($"SELECT COUNT(DISTINCT s.StockId) {filter}", parameters, cancellationToken: cancellationToken));
        const string sql = """
SELECT s.StockCode AS Code, s.StockName AS Name, e.ExchangeCode, s.ListingDate, s.IsActive
FROM Basic.Stock s INNER JOIN Basic.Exchange e ON e.ExchangeId = s.ExchangeId
LEFT JOIN Basic.StockIndustry si ON si.StockId = s.StockId
LEFT JOIN Basic.Industry i ON i.IndustryId = si.IndustryId
WHERE (@Market IS NULL OR s.MarketType = @Market)
  AND (@Industry IS NULL OR i.IndustryCode = @Industry OR i.IndustryName = @Industry)
ORDER BY s.StockCode OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
""";
        var items = (await connection.QueryAsync<StockDto>(new CommandDefinition(sql, new { parameters.Market, parameters.Industry, Offset = (pageIndex - 1) * pageSize, PageSize = pageSize }, cancellationToken: cancellationToken))).AsList();
        return new PagedResult<StockDto>(total, items);
    }

    public async Task<StockDto?> GetStockAsync(string code, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT s.StockCode AS Code, s.StockName AS Name, e.ExchangeCode, s.ListingDate, s.IsActive FROM Basic.Stock s INNER JOIN Basic.Exchange e ON e.ExchangeId = s.ExchangeId WHERE s.StockCode = @Code";
        return await connection.QuerySingleOrDefaultAsync<StockDto>(new CommandDefinition(sql, new { Code = code }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<DailyPriceDto>> GetDailyPricesAsync(string code, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
SELECT s.StockCode, p.TradeDate, p.OpenPrice, p.HighPrice, p.LowPrice, p.ClosePrice, p.Volume
FROM Market.StockDailyPrice p INNER JOIN Basic.Stock s ON s.StockId = p.StockId
WHERE s.StockCode = @Code AND (@StartDate IS NULL OR p.TradeDate >= @StartDate) AND (@EndDate IS NULL OR p.TradeDate <= @EndDate)
ORDER BY p.TradeDate;
""";
        return (await connection.QueryAsync<DailyPriceDto>(new CommandDefinition(sql, new { Code = code, StartDate = startDate, EndDate = endDate }, cancellationToken: cancellationToken))).AsList();
    }

    public async Task<IReadOnlyList<InvestmentScoreDto>> GetRankingAsync(DateOnly scoreDate, decimal? minScore, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "EXEC dbo.sp_GetStockRanking @ModelId, @ScoreDate, @TopN, @MinScore";
        return (await connection.QueryAsync<InvestmentScoreDto>(new CommandDefinition(sql, new { ModelId = 1, ScoreDate = scoreDate, TopN = 100, MinScore = minScore }, cancellationToken: cancellationToken))).AsList();
    }

    public async Task<InvestmentScoreDto?> GetLatestScoreAsync(string code, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
SELECT TOP (1)
    s.StockCode,
    score.ScoreDate,
    score.BuffettScore,
    score.GrahamScore,
    score.FisherScore,
    score.FinalScore
FROM dbo.v_StockLatestInvestmentScore score
INNER JOIN Basic.Stock s ON s.StockId = score.StockId
WHERE s.StockCode = @Code
ORDER BY score.ScoreDate DESC;
""";
        return await connection.QuerySingleOrDefaultAsync<InvestmentScoreDto>(new CommandDefinition(sql, new { Code = code }, cancellationToken: cancellationToken));
    }

    public async Task<FinancialSnapshotDto?> GetFinancialSnapshotAsync(string code, DateOnly asOfDate, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
SELECT
    s.StockCode,
    r.ReportPeriod AS AsOfDate,
    i.ROE AS Roe,
    i.ROIC AS Roic,
    i.GrossMargin,
    i.NetMargin,
    i.OperatingCashFlowToNetProfit,
    i.DebtRatio AS DebtAssetRatio,
    i.CurrentRatio,
    pe.PE,
    pb.PB,
    pe.EarningsPerShare AS Eps,
    pb.BookValuePerShare AS Bvps,
    mv.CurrentPrice AS MarketPrice,
    i.RevenueGrowth AS RevenueGrowth3Y,
    i.NetProfitGrowth AS ProfitGrowth3Y,
    CAST(0 AS DECIMAL(18, 6)) AS ResearchExpenseRatio
FROM Basic.Stock s
OUTER APPLY
(
    SELECT TOP (1) r.ReportId, r.ReportPeriod
    FROM Finance.FinancialReport r
    WHERE r.StockId = s.StockId
      AND r.PublishDate IS NOT NULL
      AND r.PublishDate <= @AsOfDate
    ORDER BY r.ReportPeriod DESC, r.PublishDate DESC, r.VersionNo DESC
) r
LEFT JOIN Finance.FinancialIndicator i ON i.ReportId = r.ReportId
OUTER APPLY
(
    SELECT TOP (1) p.PE, p.EarningsPerShare
    FROM Valuation.PERatio p
    WHERE p.StockId = s.StockId AND p.TradeDate <= @AsOfDate
    ORDER BY p.TradeDate DESC
) pe
OUTER APPLY
(
    SELECT TOP (1) p.PB, p.BookValuePerShare
    FROM Valuation.PBRatio p
    WHERE p.StockId = s.StockId AND p.TradeDate <= @AsOfDate
    ORDER BY p.TradeDate DESC
) pb
OUTER APPLY
(
    SELECT TOP (1) v.CurrentPrice
    FROM Valuation.MarketValue v
    WHERE v.StockId = s.StockId AND v.TradeDate <= @AsOfDate
    ORDER BY v.TradeDate DESC
) mv
WHERE s.StockCode = @StockCode
  AND r.ReportId IS NOT NULL;
""";
        return await connection.QuerySingleOrDefaultAsync<FinancialSnapshotDto>(new CommandDefinition(sql, new { StockCode = code, AsOfDate = asOfDate }, cancellationToken: cancellationToken));
    }

    public async Task SaveInvestmentScoreAsync(InvestmentScoreDto score, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
MERGE Quant.InvestmentScore AS target
USING (SELECT StockId FROM Basic.Stock WHERE StockCode = @StockCode) AS source
ON target.StockId = source.StockId AND target.ScoreDate = @ScoreDate
WHEN MATCHED THEN UPDATE SET BuffettScore=@BuffettScore, GrahamScore=@GrahamScore, FisherScore=@FisherScore, BaseScore=@FinalScore, FinalScore=@FinalScore, UpdatedTime=SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (StockId, ModelId, ScoreDate, BuffettScore, GrahamScore, FisherScore, ValuationScore, IndustryScore, RiskAdjustment, BaseScore, FinalScore, CreatedTime, UpdatedTime)
VALUES (source.StockId, 1, @ScoreDate, @BuffettScore, @GrahamScore, @FisherScore, @GrahamScore, 0, 0, @FinalScore, @FinalScore, SYSUTCDATETIME(), SYSUTCDATETIME());
""";
        await connection.ExecuteAsync(new CommandDefinition(sql, score, cancellationToken: cancellationToken));
    }
}