/*
====================================================
 08_VerifyDatabase.sql
 AStockQuant Database Verification
====================================================
*/

USE AStockQuant;
GO

----Schema 检查
SELECT
    s.name AS SchemaName
FROM sys.schemas s
WHERE s.name IN
(
    'Basic',
    'Market',
    'Finance',
    'Buffett',
    'Graham',
    'Fisher',
    'Valuation',
    'Industry',
    'Risk',
    'Quant',
    'Strategy',
    'Portfolio',
    'System'
)
ORDER BY s.name;
GO

----表数量检查
SELECT
    COUNT(*) AS TableCount
FROM sys.tables t
INNER JOIN sys.schemas s
    ON s.schema_id = t.schema_id
WHERE s.name IN
(
    'Basic',
    'Market',
    'Finance',
    'Buffett',
    'Graham',
    'Fisher',
    'Valuation',
    'Industry',
    'Risk',
    'Quant',
    'Strategy',
    'Portfolio',
    'System'
);
GO

----关键表存在性检查
DECLARE @RequiredTables TABLE
(
    SchemaName SYSNAME,
    TableName SYSNAME
);

INSERT INTO @RequiredTables
VALUES
('Basic','Stock'),
('Market','StockDailyPrice'),
('Finance','FinancialReport'),
('Finance','IncomeStatement'),
('Finance','BalanceSheet'),
('Finance','CashFlowStatement'),
('Finance','FinancialIndicator'),
('Finance','Dividend'),
('Finance','LongTermMetric'),

('Buffett','Indicator'),
('Buffett','ScoreRule'),
('Buffett','StockIndicatorValue'),
('Buffett','StockScore'),

('Graham','Indicator'),
('Graham','ScoreRule'),
('Graham','StockIndicatorValue'),
('Graham','StockScore'),

('Fisher','Indicator'),
('Fisher','ScoreRule'),
('Fisher','StockIndicatorValue'),
('Fisher','StockScore'),

('Valuation','MarketValue'),
('Valuation','PERatio'),
('Valuation','PBRatio'),
('Valuation','HistoricalPercentile'),
('Valuation','GrahamValue'),
('Valuation','DCFModel'),
('Valuation','DCFProjection'),
('Valuation','DCFResult'),
('Valuation','IntrinsicValue'),

('Quant','ScoreModel'),
('Quant','ScoreModelWeight'),
('Quant','InvestmentScore'),

('Strategy','StockPool'),
('Strategy','StockPoolItem'),
('Strategy','BacktestStrategy'),
('Strategy','BacktestRun'),
('Strategy','BacktestTrade'),

('Portfolio','Account'),
('Portfolio','Position'),

('System','DataSource'),
('System','DataImportTask'),
('System','DataImportLog');

SELECT
    r.SchemaName,
    r.TableName,
    CASE
        WHEN t.object_id IS NOT NULL
        THEN 'OK'
        ELSE 'MISSING'
    END AS Status
FROM @RequiredTables r
LEFT JOIN sys.schemas s
    ON s.name = r.SchemaName
LEFT JOIN sys.tables t
    ON t.schema_id = s.schema_id
    AND t.name = r.TableName
ORDER BY
    r.SchemaName,
    r.TableName;
GO

----Buffett / Graham / Fisher 指标数量
--目标:
--Buffett = 8
--Graham  = 8
--Fisher  = 10

SELECT
    'Buffett' AS Model,
    COUNT(*) AS IndicatorCount
FROM Buffett.Indicator

UNION ALL

SELECT
    'Graham',
    COUNT(*)
FROM Graham.Indicator

UNION ALL

SELECT
    'Fisher',
    COUNT(*)
FROM Fisher.Indicator;
GO

----Fisher F01～F10 完整性检查
--验证必须包含:
--F01
--F02
--F03
--F04
--F05
--F06
--F07
--F08
--F09
--F10
SELECT
    IndicatorCode,
    IndicatorName,
    Weight
FROM Fisher.Indicator
ORDER BY IndicatorCode;
GO

----三个模型权重检查
--目标:
--Buffett = 100
--Graham  = 100
--Fisher  = 100
SELECT
    'Buffett' AS Model,
    SUM(Weight) AS TotalWeight
FROM Buffett.Indicator

UNION ALL

SELECT
    'Graham',
    SUM(Weight)
FROM Graham.Indicator

UNION ALL

SELECT
    'Fisher',
    SUM(Weight)
FROM Fisher.Indicator;
GO

----综合模型权重检查
--目标:
--VALUE_INVESTMENT / V2.0 = 1.000000(就是100%)
SELECT
    m.ModelCode,
    m.Version,
    SUM(w.Weight) AS TotalWeight
FROM Quant.ScoreModel m
INNER JOIN Quant.ScoreModelWeight w
    ON w.ModelId = m.ModelId
GROUP BY
    m.ModelCode,
    m.Version;
GO

----外键检查
DBCC CHECKCONSTRAINTS;
GO

----查找孤立股票行业关系
--正确结果:
--0 rows
SELECT si.*
FROM Basic.StockIndustry si
LEFT JOIN Basic.Stock s
    ON s.StockId = si.StockId
LEFT JOIN Basic.Industry i
    ON i.IndustryId = si.IndustryId
WHERE s.StockId IS NULL
   OR i.IndustryId IS NULL;
GO

----查找孤立财务数据
--正确结果:
--0 rows
SELECT fr.*
FROM Finance.FinancialReport fr
LEFT JOIN Basic.Stock s
    ON s.StockId = fr.StockId
WHERE s.StockId IS NULL;
GO

----查找评分规则孤立记录
SELECT sr.*
FROM Buffett.ScoreRule sr
LEFT JOIN Buffett.Indicator i
    ON i.IndicatorId = sr.IndicatorId
WHERE i.IndicatorId IS NULL;
GO

SELECT sr.*
FROM Graham.ScoreRule sr
LEFT JOIN Graham.Indicator i
    ON i.IndicatorId = sr.IndicatorId
WHERE i.IndicatorId IS NULL;
GO

SELECT sr.*
FROM Fisher.ScoreRule sr
LEFT JOIN Fisher.Indicator i
    ON i.IndicatorId = sr.IndicatorId
WHERE i.IndicatorId IS NULL;
GO

----评分规则范围检查
SELECT
    'Buffett' AS Model,
    RuleId,
    MinValue,
    MaxValue
FROM Buffett.ScoreRule
WHERE MaxValue IS NOT NULL
  AND MinValue IS NOT NULL
  AND MaxValue < MinValue

UNION ALL

SELECT
    'Graham',
    RuleId,
    MinValue,
    MaxValue
FROM Graham.ScoreRule
WHERE MaxValue IS NOT NULL
  AND MinValue IS NOT NULL
  AND MaxValue < MinValue

UNION ALL

SELECT
    'Fisher',
    RuleId,
    MinValue,
    MaxValue
FROM Fisher.ScoreRule
WHERE MaxValue IS NOT NULL
  AND MinValue IS NOT NULL
  AND MaxValue < MinValue;
GO

----查询重复综合评分
--正确结果:
--0 rows
SELECT
    StockId,
    ModelId,
    ScoreDate,
    COUNT(*) AS DuplicateCount
FROM Quant.InvestmentScore
GROUP BY
    StockId,
    ModelId,
    ScoreDate
HAVING COUNT(*) > 1;
GO

----最终数据库状态
/*

                    AStockQuant
                         │
           ┌─────────────┴─────────────┐
           │                           │
       原始事实数据                  系统数据
           │                           │
     ┌─────┼─────┐               System.Import
     │     │     │
   Market Finance Basic
           │
           ↓
   FinancialIndicator
           │
           ↓
     LongTermMetric
           │
     ┌─────┼─────┐
     ↓     ↓     ↓
 Buffett Graham Fisher
     │     │     │
     └─────┼─────┘
           ↓
       Valuation
           ↓
       Industry
           ↓
          Risk
           ↓
    Quant.InvestmentScore
           ↓
        Strategy
           ↓
     ┌─────┴─────┐
     ↓           ↓
  Backtest    StockPool
                 │
                 ↓
             Portfolio

*/
