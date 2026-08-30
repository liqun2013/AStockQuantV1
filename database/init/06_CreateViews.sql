/*

v_StockBasic
      │
      ├── v_StockLatestPrice
      │
      ├── v_StockLatestFinancialQuality
      │
      ├── v_StockLatestValuation
      │
      ├── v_StockLatestRisk
      │
      └── v_StockLatestInvestmentScore
                    │
                    ↓
          v_StockInvestmentOverview

*/
/*
====================================================
 v_StockBasic最新股票基础信息
====================================================
*/

USE AStockQuant;
GO

CREATE VIEW dbo.v_StockBasic
AS
SELECT
    s.StockId,
    s.StockCode,
    s.StockName,
    e.ExchangeCode,
    e.ExchangeName,
    s.ListingDate,
    s.SecurityType,
    s.MarketType,
    s.IsActive
FROM Basic.Stock s
INNER JOIN Basic.Exchange e
    ON e.ExchangeId = s.ExchangeId;
GO
/*
====================================================
 v_StockLatestPrice最新股票行情
====================================================
*/

CREATE VIEW dbo.v_StockLatestPrice
AS
WITH RankedPrice AS
(
    SELECT
        p.*,
        ROW_NUMBER() OVER
        (
            PARTITION BY p.StockId
            ORDER BY p.TradeDate DESC
        ) AS RowNo
    FROM Market.StockDailyPrice p
)
SELECT
    StockId,
    TradeDate,
    OpenPrice,
    HighPrice,
    LowPrice,
    ClosePrice,
    ChangePercent,
    Volume,
    Amount,
    TurnoverRate,
    TotalMarketCap,
    FloatMarketCap
FROM RankedPrice
WHERE RowNo = 1;
GO
/*
====================================================
 v_StockLatestInvestmentScore最新综合投资评分
====================================================
*/

CREATE VIEW dbo.v_StockLatestInvestmentScore
AS
WITH RankedScore AS
(
    SELECT
        i.*,
        ROW_NUMBER() OVER
        (
            PARTITION BY i.StockId, i.ModelId
            ORDER BY i.ScoreDate DESC
        ) AS RowNo
    FROM Quant.InvestmentScore i
)
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
    i.RiskAdjustment,
    i.BaseScore,
    i.FinalScore,
    i.Rating,
    i.InvestmentSignal,
    i.DataAsOfDate
FROM RankedScore i
INNER JOIN Quant.ScoreModel m
    ON m.ModelId = i.ModelId
WHERE i.RowNo = 1;
GO
/*
====================================================
 v_StockInvestmentOverview综合股票分析视图
====================================================
*/

CREATE VIEW dbo.v_StockInvestmentOverview
AS
SELECT
    s.StockId,
    s.StockCode,
    s.StockName,

    p.TradeDate,
    p.ClosePrice,
    p.ChangePercent,
    p.Amount,
    p.TotalMarketCap,

    score.ModelCode,
    score.ModelVersion,

    score.BuffettScore,
    score.GrahamScore,
    score.FisherScore,
    score.ValuationScore,
    score.IndustryScore,

    score.BaseScore,
    score.RiskAdjustment,
    score.FinalScore,

    score.Rating,
    score.InvestmentSignal

FROM Basic.Stock s

LEFT JOIN dbo.v_StockLatestPrice p
    ON p.StockId = s.StockId

LEFT JOIN dbo.v_StockLatestInvestmentScore score
    ON score.StockId = s.StockId
WHERE s.IsActive = 1;
GO
/*
====================================================
 v_StockLatestValuation 最新估值视图
====================================================
*/

CREATE VIEW dbo.v_StockLatestValuation
AS
WITH PE AS
(
    SELECT
        p.*,
        ROW_NUMBER() OVER
        (
            PARTITION BY p.StockId
            ORDER BY p.TradeDate DESC
        ) AS RowNo
    FROM Valuation.PERatio p
),
PB AS
(
    SELECT
        p.*,
        ROW_NUMBER() OVER
        (
            PARTITION BY p.StockId
            ORDER BY p.TradeDate DESC
        ) AS RowNo
    FROM Valuation.PBRatio p
),
PS AS
(
    SELECT
        p.*,
        ROW_NUMBER() OVER
        (
            PARTITION BY p.StockId
            ORDER BY p.TradeDate DESC
        ) AS RowNo
    FROM Valuation.PSRatio p
),
DY AS
(
    SELECT
        p.*,
        ROW_NUMBER() OVER
        (
            PARTITION BY p.StockId
            ORDER BY p.TradeDate DESC
        ) AS RowNo
    FROM Valuation.DividendYield p
)
SELECT
    s.StockId,
    s.StockCode,
    s.StockName,

    pe.TradeDate AS PETradeDate,
    pe.PE,
    pe.PETTM,
    pe.PEForward,

    pb.PB,

    ps.PS,

    dy.DividendYield

FROM Basic.Stock s

LEFT JOIN PE pe
    ON pe.StockId = s.StockId
    AND pe.RowNo = 1

LEFT JOIN PB pb
    ON pb.StockId = s.StockId
    AND pb.RowNo = 1

LEFT JOIN PS ps
    ON ps.StockId = s.StockId
    AND ps.RowNo = 1

LEFT JOIN DY dy
    ON dy.StockId = s.StockId
    AND dy.RowNo = 1

WHERE s.IsActive = 1;
GO
/*
====================================================
 v_StockLatestFinancialQuality最新财务质量视图
====================================================
*/

CREATE VIEW dbo.v_StockLatestFinancialQuality
AS
WITH RankedFinancial AS
(
    SELECT
        f.*,

        ROW_NUMBER() OVER
        (
            PARTITION BY f.StockId
            ORDER BY r.ReportPeriod DESC,
                     r.PublishDate DESC,
                     r.VersionNo DESC
        ) AS RowNo

    FROM Finance.FinancialIndicator f

    INNER JOIN Finance.FinancialReport r
        ON r.ReportId = f.ReportId
)
SELECT
    f.StockId,
    f.ReportId,
    r.ReportPeriod,
    r.ReportType,
    r.PublishDate,

    f.ROE,
    f.ROA,
    f.ROIC,

    f.GrossMargin,
    f.OperatingMargin,
    f.NetMargin,

    f.DebtRatio,
    f.CurrentRatio,
    f.QuickRatio,

    f.RevenueGrowth,
    f.NetProfitGrowth,

    f.OperatingCashFlowToNetProfit,

    f.FreeCashFlow,
    f.FreeCashFlowMargin

FROM RankedFinancial f

INNER JOIN Finance.FinancialReport r
    ON r.ReportId = f.ReportId

WHERE f.RowNo = 1;
GO
/*
====================================================
 v_StockLatestRisk最新风险视图
====================================================
*/

CREATE VIEW dbo.v_StockLatestRisk
AS
WITH RankedRisk AS
(
    SELECT
        r.*,
        ROW_NUMBER() OVER
        (
            PARTITION BY r.StockId
            ORDER BY r.CalcDate DESC
        ) AS RowNo
    FROM Risk.StockRiskIndicator r
)
SELECT
    StockId,
    CalcDate,
    Volatility30D,
    Volatility60D,
    Volatility252D,
    MaxDrawdown1Y,
    MaxDrawdown3Y,
    Beta,
    DebtRiskScore,
    LiquidityRiskScore,
    ValuationRiskScore,
    FinancialRiskScore,
    TotalRiskScore,
    RiskLevel
FROM RankedRisk
WHERE RowNo = 1;
GO
