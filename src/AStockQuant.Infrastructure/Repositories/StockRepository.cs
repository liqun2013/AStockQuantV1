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
        var rows = await connection.QueryAsync(new CommandDefinition(sql, new
        {
            Code = code,
            StartDate = startDate?.ToDateTime(TimeOnly.MinValue),
            EndDate = endDate?.ToDateTime(TimeOnly.MinValue)
        }, cancellationToken: cancellationToken));
        return rows.Select(row => new DailyPriceDto(
            (string)row.StockCode,
            DateOnly.FromDateTime(Convert.ToDateTime(row.TradeDate)),
            Convert.ToDecimal(row.OpenPrice),
            Convert.ToDecimal(row.HighPrice),
            Convert.ToDecimal(row.LowPrice),
            Convert.ToDecimal(row.ClosePrice),
            Convert.ToInt64(row.Volume))).ToArray();
    }

    public async Task<IReadOnlyList<InvestmentScoreDto>> GetRankingAsync(DateOnly scoreDate, decimal? minScore, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "EXEC dbo.sp_GetStockRanking @ModelId, @ScoreDate, @TopN, @MinScore";
        var rows = await connection.QueryAsync(new CommandDefinition(sql, new
        {
            ModelId = 1,
            ScoreDate = scoreDate.ToDateTime(TimeOnly.MinValue),
            TopN = 100,
            MinScore = minScore
        }, cancellationToken: cancellationToken));
        return rows.Select(ToInvestmentScore).ToArray();
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
        var row = await connection.QuerySingleOrDefaultAsync(new CommandDefinition(sql, new { Code = code }, cancellationToken: cancellationToken));
        return row is null ? null : ToInvestmentScore(row);
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
        var row = await connection.QuerySingleOrDefaultAsync(new CommandDefinition(sql, new
        {
            StockCode = code,
            AsOfDate = asOfDate.ToDateTime(TimeOnly.MinValue)
        }, cancellationToken: cancellationToken));
        return row is null ? null : new FinancialSnapshotDto(
            (string)row.StockCode,
            DateOnly.FromDateTime(Convert.ToDateTime(row.AsOfDate)),
            Convert.ToDecimal(row.Roe),
            Convert.ToDecimal(row.Roic),
            Convert.ToDecimal(row.GrossMargin),
            Convert.ToDecimal(row.NetMargin),
            Convert.ToDecimal(row.OperatingCashFlowToNetProfit),
            Convert.ToDecimal(row.DebtAssetRatio),
            Convert.ToDecimal(row.CurrentRatio),
            Convert.ToDecimal(row.PE),
            Convert.ToDecimal(row.PB),
            Convert.ToDecimal(row.Eps),
            Convert.ToDecimal(row.Bvps),
            Convert.ToDecimal(row.MarketPrice),
            Convert.ToDecimal(row.RevenueGrowth3Y),
            Convert.ToDecimal(row.ProfitGrowth3Y),
            Convert.ToDecimal(row.ResearchExpenseRatio));
    }

    private static InvestmentScoreDto ToInvestmentScore(dynamic row) => new(
        (string)row.StockCode,
        DateOnly.FromDateTime(Convert.ToDateTime(row.ScoreDate)),
        Convert.ToDecimal(row.BuffettScore),
        Convert.ToDecimal(row.GrahamScore),
        Convert.ToDecimal(row.FisherScore),
        Convert.ToDecimal(row.FinalScore));

    public async Task SaveInvestmentScoreAsync(InvestmentScoreDto score, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
MERGE Quant.InvestmentScore AS target
USING (SELECT StockId FROM Basic.Stock WHERE StockCode = @StockCode) AS source
ON target.StockId = source.StockId AND target.ModelId = 1 AND target.ScoreDate = @ScoreDate
WHEN MATCHED THEN UPDATE SET BuffettScore=@BuffettScore, GrahamScore=@GrahamScore, FisherScore=@FisherScore, BaseScore=@FinalScore, FinalScore=@FinalScore
WHEN NOT MATCHED THEN INSERT (StockId, ModelId, ScoreDate, BuffettScore, GrahamScore, FisherScore, ValuationScore, IndustryScore, RiskAdjustment, BaseScore, FinalScore, DataAsOfDate, CreatedTime)
VALUES (source.StockId, 1, @ScoreDate, @BuffettScore, @GrahamScore, @FisherScore, @GrahamScore, 0, 0, @FinalScore, @FinalScore, @ScoreDate, SYSUTCDATETIME());
""";
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            score.StockCode,
            ScoreDate = score.ScoreDate.ToDateTime(TimeOnly.MinValue),
            score.BuffettScore,
            score.GrahamScore,
            score.FisherScore,
            score.FinalScore
        }, cancellationToken: cancellationToken));
    }
}