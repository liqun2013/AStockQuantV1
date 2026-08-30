/*
====================================================
 Finance.LongTermMetric
 长期派生财务指标
====================================================
*/

USE AStockQuant;
GO

CREATE TABLE Finance.LongTermMetric
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Finance_LongTermMetric PRIMARY KEY,

    StockId INT NOT NULL,

    CalcDate DATE NOT NULL,

    MetricCode VARCHAR(50) NOT NULL,

    MetricValue DECIMAL(24,8) NULL,

    PeriodYears INT NULL,

    StartReportPeriod DATE NULL,

    EndReportPeriod DATE NULL,

    DataAsOfDate DATE NULL,

    ModelVersion VARCHAR(30) NOT NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Finance_LongTermMetric_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Finance_LongTermMetric_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT UQ_Finance_LongTermMetric
        UNIQUE
        (
            StockId,
            CalcDate,
            MetricCode,
            ModelVersion
        )
);
GO

CREATE TABLE Finance.LongTermMetricDefinition
(
    MetricId INT IDENTITY(1,1)
        CONSTRAINT PK_Finance_LongTermMetricDefinition
        PRIMARY KEY,

    MetricCode VARCHAR(50) NOT NULL,

    MetricName NVARCHAR(100) NOT NULL,

    Unit VARCHAR(30) NULL,

    Description NVARCHAR(1000) NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Finance_LongTermMetricDefinition_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Finance_LongTermMetricDefinition_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Finance_LongTermMetricDefinition_Code
        UNIQUE (MetricCode)
);
GO

----初始化长期指标定义
INSERT INTO Finance.LongTermMetricDefinition
(
    MetricCode,
    MetricName,
    Unit,
    Description
)
VALUES
('RevenueCAGR3Y', N'3年营业收入复合增长率', '%', N'过去3年营业收入CAGR'),
('RevenueCAGR5Y', N'5年营业收入复合增长率', '%', N'过去5年营业收入CAGR'),
('RevenueCAGR10Y', N'10年营业收入复合增长率', '%', N'过去10年营业收入CAGR'),

('NetProfitCAGR3Y', N'3年净利润复合增长率', '%', N'过去3年净利润CAGR'),
('NetProfitCAGR5Y', N'5年净利润复合增长率', '%', N'过去5年净利润CAGR'),
('NetProfitCAGR10Y', N'10年净利润复合增长率', '%', N'过去10年净利润CAGR'),

('AverageROE3Y', N'3年平均ROE', '%', N'过去3年平均ROE'),
('AverageROE5Y', N'5年平均ROE', '%', N'过去5年平均ROE'),
('AverageROE10Y', N'10年平均ROE', '%', N'过去10年平均ROE'),

('AverageROIC3Y', N'3年平均ROIC', '%', N'过去3年平均ROIC'),
('AverageROIC5Y', N'5年平均ROIC', '%', N'过去5年平均ROIC'),
('AverageROIC10Y', N'10年平均ROIC', '%', N'过去10年平均ROIC'),

('FCFPositiveRatio5Y', N'5年自由现金流为正比例', '%', N'过去5年FCF为正的年份比例'),
('FCFPositiveRatio10Y', N'10年自由现金流为正比例', '%', N'过去10年FCF为正的年份比例'),

('FCFCAGR5Y', N'5年自由现金流复合增长率', '%', N'过去5年FCF CAGR'),

('DividendGrowth5Y', N'5年股息增长率', '%', N'过去5年股息复合增长率'),
('DividendGrowth10Y', N'10年股息增长率', '%', N'过去10年股息复合增长率'),

('EPSCAGR5Y', N'5年EPS复合增长率', '%', N'过去5年EPS CAGR'),
('EPSCAGR10Y', N'10年EPS复合增长率', '%', N'过去10年EPS CAGR'),

('GrossMarginAverage5Y', N'5年平均毛利率', '%', N'过去5年平均毛利率'),
('NetMarginAverage5Y', N'5年平均净利率', '%', N'过去5年平均净利率'),

('DebtRatioAverage5Y', N'5年平均资产负债率', '%', N'过去5年平均资产负债率');
GO

CREATE INDEX IX_Finance_LongTermMetric_Stock_Date
ON Finance.LongTermMetric
(
    StockId,
    CalcDate DESC,
    MetricCode
)
INCLUDE
(
    MetricValue,
    PeriodYears,
    StartReportPeriod,
    EndReportPeriod,
    DataAsOfDate,
    ModelVersion
);
GO

/*

AKTools / AKShare
        ↓
原始行情 / 财务
        ↓
Finance
        ↓
FinancialIndicator
        ↓
LongTermMetric
        ↓
┌─────────────┬─────────────┬─────────────┐
↓             ↓             ↓
Buffett       Fisher        Valuation
↓             ↓             ↓
      Quant 综合评分
             ↓
       Strategy / Backtest
             ↓
       Portfolio

*/