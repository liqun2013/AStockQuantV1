/*
====================================================
 07_CreateStoredProcedures.sql
 SQL Server 2019

 Procedure:
 sp_GetStockFinancialAsOfDate

 Purpose:
 获取截至指定日期，投资者当时能够看到的
 最新财务数据。

 Critical Rule:
 PublishDate <= @AsOfDate

 用于避免 Look-ahead Bias。
====================================================
*/

USE AStockQuant;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetStockFinancialAsOfDate
(
    @StockId INT,
    @AsOfDate DATE
)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH RankedReports AS
    (
        SELECT
            r.ReportId,
            r.StockId,
            r.ReportPeriod,
            r.ReportType,
            r.PublishDate,
            r.VersionNo,

            ROW_NUMBER() OVER
            (
                PARTITION BY r.StockId
                ORDER BY
                    r.ReportPeriod DESC,
                    r.PublishDate DESC,
                    r.VersionNo DESC
            ) AS RowNo

        FROM Finance.FinancialReport r
        WHERE r.StockId = @StockId
          AND r.PublishDate IS NOT NULL
          AND r.PublishDate <= @AsOfDate
    )
    SELECT
        r.ReportId,
        r.StockId,
        r.ReportPeriod,
        r.ReportType,
        r.PublishDate,

        i.ROE,
        i.ROA,
        i.ROIC,

        i.GrossMargin,
        i.OperatingMargin,
        i.NetMargin,

        i.DebtRatio,
        i.CurrentRatio,
        i.QuickRatio,

        i.RevenueGrowth,
        i.NetProfitGrowth,

        i.OperatingCashFlowGrowth,
        i.OperatingCashFlowToNetProfit,

        i.FreeCashFlow,
        i.FreeCashFlowMargin

    FROM RankedReports r

    LEFT JOIN Finance.FinancialIndicator i
        ON i.ReportId = r.ReportId

    WHERE r.RowNo = 1;
END;
GO
--EXEC dbo.sp_GetStockFinancialAsOfDate
--     @StockId = 100,
--     @AsOfDate = '2025-06-30';

----获取完整股票分析数据
CREATE OR ALTER PROCEDURE dbo.sp_GetStockAnalysis
(
    @StockId INT,
    @AsOfDate DATE = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @AsOfDate IS NULL
        SET @AsOfDate = CAST(GETDATE() AS DATE);

    /*
    ------------------------------------------------
    1. 股票基础信息
    ------------------------------------------------
    */

    SELECT
        s.StockId,
        s.StockCode,
        s.StockName,
        e.ExchangeCode,
        e.ExchangeName,
        s.ListingDate,
        s.SecurityType,
        s.MarketType
    FROM Basic.Stock s
    INNER JOIN Basic.Exchange e
        ON e.ExchangeId = s.ExchangeId
    WHERE s.StockId = @StockId;


    /*
    ------------------------------------------------
    2. 截至指定日期的最新行情
    ------------------------------------------------
    */

    SELECT TOP (1)
        p.StockId,
        p.TradeDate,
        p.OpenPrice,
        p.HighPrice,
        p.LowPrice,
        p.ClosePrice,
        p.ChangePercent,
        p.Volume,
        p.Amount,
        p.TurnoverRate,
        p.TotalMarketCap,
        p.FloatMarketCap
    FROM Market.StockDailyPrice p
    WHERE p.StockId = @StockId
      AND p.TradeDate <= @AsOfDate
    ORDER BY p.TradeDate DESC;


    /*
    ------------------------------------------------
    3. 最新可用财务数据
    ------------------------------------------------
    */

    EXEC dbo.sp_GetStockFinancialAsOfDate
         @StockId = @StockId,
         @AsOfDate = @AsOfDate;


    /*
    ------------------------------------------------
    4. 最新估值
    ------------------------------------------------
    */

    SELECT TOP (1)
        pe.TradeDate,
        pe.PE,
        pe.PETTM,
        pe.PEForward
    FROM Valuation.PERatio pe
    WHERE pe.StockId = @StockId
      AND pe.TradeDate <= @AsOfDate
    ORDER BY pe.TradeDate DESC;


    SELECT TOP (1)
        pb.TradeDate,
        pb.PB
    FROM Valuation.PBRatio pb
    WHERE pb.StockId = @StockId
      AND pb.TradeDate <= @AsOfDate
    ORDER BY pb.TradeDate DESC;


    SELECT TOP (1)
        ps.TradeDate,
        ps.PS
    FROM Valuation.PSRatio ps
    WHERE ps.StockId = @StockId
      AND ps.TradeDate <= @AsOfDate
    ORDER BY ps.TradeDate DESC;


    SELECT TOP (1)
        dy.TradeDate,
        dy.DividendYield
    FROM Valuation.DividendYield dy
    WHERE dy.StockId = @StockId
      AND dy.TradeDate <= @AsOfDate
    ORDER BY dy.TradeDate DESC;


    /*
    ------------------------------------------------
    5. Buffett / Graham / Fisher 最新评分
    ------------------------------------------------
    */

    SELECT TOP (1)
        StockId,
        ScoreDate,
        TotalScore,
        Grade,
        ModelVersion
    FROM Buffett.StockScore
    WHERE StockId = @StockId
      AND ScoreDate <= @AsOfDate
    ORDER BY ScoreDate DESC;


    SELECT TOP (1)
        StockId,
        ScoreDate,
        TotalScore,
        Grade,
        ModelVersion
    FROM Graham.StockScore
    WHERE StockId = @StockId
      AND ScoreDate <= @AsOfDate
    ORDER BY ScoreDate DESC;


    SELECT TOP (1)
        StockId,
        ScoreDate,
        TotalScore,
        Grade,
        ModelVersion
    FROM Fisher.StockScore
    WHERE StockId = @StockId
      AND ScoreDate <= @AsOfDate
    ORDER BY ScoreDate DESC;


    /*
    ------------------------------------------------
    6. 综合评分
    ------------------------------------------------
    */

    SELECT TOP (1)
        i.StockId,
        i.ModelId,
        m.ModelCode,
        m.ModelName,
        m.Version AS ModelVersion,

        i.ScoreDate,

        i.BuffettScore,
        i.GrahamScore,
        i.FisherScore,
        i.ValuationScore,
        i.IndustryScore,

        i.BaseScore,
        i.RiskAdjustment,
        i.FinalScore,

        i.Rating,
        i.InvestmentSignal

    FROM Quant.InvestmentScore i
    INNER JOIN Quant.ScoreModel m
        ON m.ModelId = i.ModelId

    WHERE i.StockId = @StockId
      AND i.ScoreDate <= @AsOfDate

    ORDER BY i.ScoreDate DESC;
END;
GO
-----股票历史评分
CREATE OR ALTER PROCEDURE dbo.sp_GetStockScoreHistory
(
    @StockId INT,
    @ModelId INT = NULL,
    @StartDate DATE = NULL,
    @EndDate DATE = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        i.ScoreId,
        i.StockId,
        i.ModelId,
        m.ModelCode,
        m.ModelName,
        m.Version AS ModelVersion,

        i.ScoreDate,

        i.BuffettScore,
        i.GrahamScore,
        i.FisherScore,
        i.ValuationScore,
        i.IndustryScore,

        i.BaseScore,
        i.RiskAdjustment,
        i.FinalScore,

        i.Rating,
        i.InvestmentSignal

    FROM Quant.InvestmentScore i

    INNER JOIN Quant.ScoreModel m
        ON m.ModelId = i.ModelId

    WHERE i.StockId = @StockId

      AND
      (
          @ModelId IS NULL
          OR i.ModelId = @ModelId
      )

      AND
      (
          @StartDate IS NULL
          OR i.ScoreDate >= @StartDate
      )

      AND
      (
          @EndDate IS NULL
          OR i.ScoreDate <= @EndDate
      )

    ORDER BY
        i.ScoreDate ASC;
END;
GO
----股票排行榜
CREATE OR ALTER PROCEDURE dbo.sp_GetStockRanking
(
    @ModelId INT,
    @ScoreDate DATE,
    @TopN INT = 100,
    @MinScore DECIMAL(10,4) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@TopN)

        i.StockId,

        s.StockCode,

        s.StockName,

        i.ScoreDate,

        i.BuffettScore,

        i.GrahamScore,

        i.FisherScore,

        i.ValuationScore,

        i.IndustryScore,

        i.BaseScore,

        i.RiskAdjustment,

        i.FinalScore,

        i.Rating,

        i.InvestmentSignal

    FROM Quant.InvestmentScore i

    INNER JOIN Basic.Stock s
        ON s.StockId = i.StockId

    WHERE i.ModelId = @ModelId

      AND i.ScoreDate = @ScoreDate

      AND
      (
          @MinScore IS NULL
          OR i.FinalScore >= @MinScore
      )

    ORDER BY
        i.FinalScore DESC,
        i.StockId;
END;
GO
--EXEC dbo.sp_GetStockRanking
--     @ModelId = 1,
--     @ScoreDate = '2026-08-24',
--     @TopN = 50,
--     @MinScore = 80;

----回测候选股票
CREATE OR ALTER PROCEDURE dbo.sp_GetBacktestCandidates
(
    @ModelId INT,
    @TradeDate DATE,
    @MinScore DECIMAL(10,4) = 80,
    @TopN INT = 50
)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH RankedCandidates AS
    (
        SELECT
            i.StockId,
            s.StockCode,
            s.StockName,

            i.ScoreDate,

            i.BuffettScore,
            i.GrahamScore,
            i.FisherScore,
            i.ValuationScore,
            i.IndustryScore,

            i.FinalScore,
            i.Rating,
            i.InvestmentSignal,

            ROW_NUMBER() OVER
            (
                ORDER BY i.FinalScore DESC
            ) AS RowNo

        FROM Quant.InvestmentScore i

        INNER JOIN Basic.Stock s
            ON s.StockId = i.StockId

        WHERE i.ModelId = @ModelId

          AND i.ScoreDate <= @TradeDate

          AND
          (
              i.DataAsOfDate IS NULL
              OR i.DataAsOfDate <= @TradeDate
          )

          AND i.FinalScore >= @MinScore
    )

    SELECT
        StockId,
        StockCode,
        StockName,
        ScoreDate,
        BuffettScore,
        GrahamScore,
        FisherScore,
        ValuationScore,
        IndustryScore,
        FinalScore,
        Rating,
        InvestmentSignal
    FROM RankedCandidates
    WHERE RowNo <= @TopN
    ORDER BY FinalScore DESC;
END;
GO

-----获取股票财务历史
CREATE OR ALTER PROCEDURE dbo.sp_GetStockFinancialHistory
(
    @StockId INT,
    @Years INT = 10
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Years)

        r.ReportId,

        r.ReportPeriod,

        r.ReportType,

        r.PublishDate,

        i.ROE,
        i.ROA,
        i.ROIC,

        i.GrossMargin,
        i.NetMargin,

        i.DebtRatio,

        i.RevenueGrowth,
        i.NetProfitGrowth,

        i.OperatingCashFlowToNetProfit,

        i.FreeCashFlow,
        i.FreeCashFlowMargin

    FROM Finance.FinancialReport r

    INNER JOIN Finance.FinancialIndicator i
        ON i.ReportId = r.ReportId

    WHERE r.StockId = @StockId

    ORDER BY
        r.ReportPeriod DESC,
        r.PublishDate DESC;
END;
GO

----获取股票历史行情
CREATE OR ALTER PROCEDURE dbo.sp_GetStockDailyPrice
(
    @StockId INT,
    @StartDate DATE,
    @EndDate DATE
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        StockId,
        TradeDate,
        OpenPrice,
        HighPrice,
        LowPrice,
        ClosePrice,
        PrevClosePrice,
        ChangeAmount,
        ChangePercent,
        Volume,
        Amount,
        TurnoverRate,
        TotalMarketCap,
        FloatMarketCap
    FROM Market.StockDailyPrice

    WHERE StockId = @StockId
      AND TradeDate >= @StartDate
      AND TradeDate <= @EndDate

    ORDER BY
        TradeDate ASC;
END;
GO

----获取投资组合当前状态
CREATE OR ALTER PROCEDURE dbo.sp_GetPortfolioOverview
(
    @AccountId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    /*
    ================================================
    当前持仓
    ================================================
    */

    SELECT
        p.PositionId,
        p.AccountId,

        s.StockCode,
        s.StockName,

        p.Quantity,
        p.AvailableQuantity,

        p.AverageCost,
        p.CurrentPrice,

        p.MarketValue,
        p.CostValue,

        p.UnrealizedProfitLoss,
        p.UnrealizedReturnRate,

        p.DividendIncome,

        p.LastUpdatedTime

    FROM Portfolio.Position p

    INNER JOIN Basic.Stock s
        ON s.StockId = p.StockId

    WHERE p.AccountId = @AccountId

      AND p.Quantity > 0

    ORDER BY
        p.MarketValue DESC;


    /*
    ================================================
    最新组合快照
    ================================================
    */

    SELECT TOP (1)

        SnapshotDate,

        CashBalance,
        MarketValue,
        TotalAssets,
        TotalCost,

        UnrealizedProfitLoss,
        RealizedProfitLoss,

        DividendIncome,

        TotalReturn,
        DailyReturn,
        Drawdown

    FROM Portfolio.PortfolioSnapshot

    WHERE AccountId = @AccountId

    ORDER BY SnapshotDate DESC;
END;
GO

-----获取导入任务状态
CREATE OR ALTER PROCEDURE dbo.sp_GetImportTaskStatus
(
    @TaskId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)

        l.ImportLogId,

        l.TaskId,

        t.TaskCode,
        t.TaskName,

        l.DataType,

        l.StartTime,
        l.EndTime,

        l.Status,

        l.RequestCount,
        l.SuccessCount,
        l.FailedCount,

        l.InsertedCount,
        l.UpdatedCount,
        l.SkippedCount,

        l.ErrorMessage,
        l.CorrelationId

    FROM System.DataImportLog l

    LEFT JOIN System.DataImportTask t
        ON t.TaskId = l.TaskId

    WHERE l.TaskId = @TaskId

    ORDER BY
        l.StartTime DESC;
END;
GO

----