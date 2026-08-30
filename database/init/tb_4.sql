/*
====================================================
 Valuation.MarketValue
 市值历史

                 FinancialReport
                       │
                       ↓
                  EPS / BVPS / FCF
                       │
          ┌────────────┼────────────┐
          ↓            ↓            ↓
         PE           PB          FCF
          │            │            │
          ↓            ↓            ↓
      Relative     Relative        DCF
      Valuation    Valuation     Valuation
          │            │            │
          └────────────┼────────────┘
                       ↓
              IntrinsicValue
                       │
                       ↓
                Safety Margin
                       │
                       ↓
                ValuationScore
====================================================
*/

USE AStockQuant;
GO

CREATE TABLE Valuation.MarketValue
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Valuation_MarketValue PRIMARY KEY,

    StockId INT NOT NULL,

    TradeDate DATE NOT NULL,

    TotalShares DECIMAL(24,6) NULL,

    FloatShares DECIMAL(24,6) NULL,

    MarketCap DECIMAL(24,4) NULL,

    FloatMarketCap DECIMAL(24,4) NULL,

    CurrentPrice DECIMAL(18,6) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Valuation_MarketValue_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Valuation_MarketValue_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT UQ_Valuation_MarketValue
        UNIQUE (StockId, TradeDate)
);
GO
/*
====================================================
 Valuation.PERatio
 PE历史估值
====================================================
*/

CREATE TABLE Valuation.PERatio
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Valuation_PERatio PRIMARY KEY,

    StockId INT NOT NULL,

    TradeDate DATE NOT NULL,

    PE DECIMAL(18,6) NULL,

    PETTM DECIMAL(18,6) NULL,

    PEForward DECIMAL(18,6) NULL,

    EarningsPerShare DECIMAL(18,8) NULL,

    SourceReportId BIGINT NULL,

    DataAsOfDate DATE NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Valuation_PERatio_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Valuation_PERatio_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT FK_Valuation_PERatio_Report
        FOREIGN KEY (SourceReportId)
        REFERENCES Finance.FinancialReport(ReportId),

    CONSTRAINT UQ_Valuation_PERatio
        UNIQUE (StockId, TradeDate)
);
GO
/*
====================================================
 Valuation.PBRatio
 PB历史估值
====================================================
*/

CREATE TABLE Valuation.PBRatio
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Valuation_PBRatio PRIMARY KEY,

    StockId INT NOT NULL,

    TradeDate DATE NOT NULL,

    PB DECIMAL(18,6) NULL,

    BookValuePerShare DECIMAL(18,8) NULL,

    SourceReportId BIGINT NULL,

    DataAsOfDate DATE NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Valuation_PBRatio_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Valuation_PBRatio_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT FK_Valuation_PBRatio_Report
        FOREIGN KEY (SourceReportId)
        REFERENCES Finance.FinancialReport(ReportId),

    CONSTRAINT UQ_Valuation_PBRatio
        UNIQUE (StockId, TradeDate)
);
GO
/*
====================================================
 Valuation.PSRatio
 PS历史估值
====================================================
*/

CREATE TABLE Valuation.PSRatio
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Valuation_PSRatio PRIMARY KEY,

    StockId INT NOT NULL,

    TradeDate DATE NOT NULL,

    PS DECIMAL(18,6) NULL,

    SalesPerShare DECIMAL(18,8) NULL,

    SourceReportId BIGINT NULL,

    DataAsOfDate DATE NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Valuation_PSRatio_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Valuation_PSRatio_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT FK_Valuation_PSRatio_Report
        FOREIGN KEY (SourceReportId)
        REFERENCES Finance.FinancialReport(ReportId),

    CONSTRAINT UQ_Valuation_PSRatio
        UNIQUE (StockId, TradeDate)
);
GO
/*
====================================================
 Valuation.DividendYield
 股息率历史
====================================================
*/

CREATE TABLE Valuation.DividendYield
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Valuation_DividendYield PRIMARY KEY,

    StockId INT NOT NULL,

    TradeDate DATE NOT NULL,

    AnnualDividendPerShare DECIMAL(18,8) NULL,

    DividendYield DECIMAL(18,8) NULL,

    SourceDividendId BIGINT NULL,

    DataAsOfDate DATE NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Valuation_DividendYield_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Valuation_DividendYield_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT FK_Valuation_DividendYield_Dividend
        FOREIGN KEY (SourceDividendId)
        REFERENCES Finance.Dividend(DividendId),

    CONSTRAINT UQ_Valuation_DividendYield
        UNIQUE (StockId, TradeDate)
);
GO
/*
====================================================
 Valuation.HistoricalPercentile
 历史估值分位
====================================================
*/

CREATE TABLE Valuation.HistoricalPercentile
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Valuation_HistoricalPercentile PRIMARY KEY,

    StockId INT NOT NULL,

    CalcDate DATE NOT NULL,

    MetricCode VARCHAR(30) NOT NULL,
    /*
        PE
        PB
        PS
        DividendYield
    */

    CurrentValue DECIMAL(18,8) NULL,

    Percentile1Y DECIMAL(10,6) NULL,

    Percentile3Y DECIMAL(10,6) NULL,

    Percentile5Y DECIMAL(10,6) NULL,

    Percentile10Y DECIMAL(10,6) NULL,

    MinValue DECIMAL(18,8) NULL,

    MaxValue DECIMAL(18,8) NULL,

    MedianValue DECIMAL(18,8) NULL,

    DataAsOfDate DATE NULL,

    ModelVersion VARCHAR(30) NOT NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Valuation_HistoricalPercentile_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Valuation_HistoricalPercentile_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT UQ_Valuation_HistoricalPercentile
        UNIQUE
        (
            StockId,
            CalcDate,
            MetricCode,
            ModelVersion
        )
);
GO
/*
====================================================
 Valuation.GrahamValue
 Graham Number
====================================================
*/

CREATE TABLE Valuation.GrahamValue
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Valuation_GrahamValue PRIMARY KEY,

    StockId INT NOT NULL,

    CalcDate DATE NOT NULL,

    EPS DECIMAL(18,8) NULL,

    BVPS DECIMAL(18,8) NULL,

    GrahamNumber DECIMAL(18,8) NULL,

    CurrentPrice DECIMAL(18,8) NULL,

    UpsideToGraham DECIMAL(18,8) NULL,

    SafetyMargin DECIMAL(18,8) NULL,

    SourceReportId BIGINT NULL,

    DataAsOfDate DATE NULL,

    ModelVersion VARCHAR(30) NOT NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Valuation_GrahamValue_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Valuation_GrahamValue_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT FK_Valuation_GrahamValue_Report
        FOREIGN KEY (SourceReportId)
        REFERENCES Finance.FinancialReport(ReportId),

    CONSTRAINT UQ_Valuation_GrahamValue
        UNIQUE
        (
            StockId,
            CalcDate,
            ModelVersion
        )
);
GO
/*
====================================================
 Valuation.DCFModel
 DCF模型参数
====================================================
*/

CREATE TABLE Valuation.DCFModel
(
    ModelId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Valuation_DCFModel PRIMARY KEY,

    StockId INT NOT NULL,

    CalcDate DATE NOT NULL,

    BaseReportId BIGINT NULL,

    BaseFreeCashFlow DECIMAL(24,4) NULL,

    ForecastYears INT NOT NULL,

    GrowthRate DECIMAL(18,8) NULL,

    GrowthRateYear1 DECIMAL(18,8) NULL,

    GrowthRateYear2 DECIMAL(18,8) NULL,

    GrowthRateYear3 DECIMAL(18,8) NULL,

    GrowthRateYear4 DECIMAL(18,8) NULL,

    GrowthRateYear5 DECIMAL(18,8) NULL,

    DiscountRate DECIMAL(18,8) NULL,

    TerminalGrowthRate DECIMAL(18,8) NULL,

    ShareCount DECIMAL(24,6) NULL,

    NetDebt DECIMAL(24,4) NULL,

    ModelVersion VARCHAR(30) NOT NULL,

    DataAsOfDate DATE NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Valuation_DCFModel_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Valuation_DCFModel_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT FK_Valuation_DCFModel_Report
        FOREIGN KEY (BaseReportId)
        REFERENCES Finance.FinancialReport(ReportId),

    CONSTRAINT CK_Valuation_DCFModel_ForecastYears
        CHECK (ForecastYears BETWEEN 1 AND 20),

    CONSTRAINT UQ_Valuation_DCFModel
        UNIQUE
        (
            StockId,
            CalcDate,
            ModelVersion
        )
);
GO
/*
====================================================
 Valuation.DCFProjection
 DCF未来现金流预测
====================================================
*/

CREATE TABLE Valuation.DCFProjection
(
    ProjectionId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Valuation_DCFProjection PRIMARY KEY,

    ModelId BIGINT NOT NULL,

    YearNumber INT NOT NULL,

    ForecastDate DATE NULL,

    FreeCashFlow DECIMAL(24,4) NULL,

    GrowthRate DECIMAL(18,8) NULL,

    DiscountFactor DECIMAL(18,10) NULL,

    PresentValue DECIMAL(24,4) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Valuation_DCFProjection_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Valuation_DCFProjection_Model
        FOREIGN KEY (ModelId)
        REFERENCES Valuation.DCFModel(ModelId),

    CONSTRAINT UQ_Valuation_DCFProjection
        UNIQUE (ModelId, YearNumber),

    CONSTRAINT CK_Valuation_DCFProjection_Year
        CHECK (YearNumber > 0)
);
GO
/*
====================================================
 Valuation.DCFResult
 DCF估值结果
====================================================
*/

CREATE TABLE Valuation.DCFResult
(
    ResultId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Valuation_DCFResult PRIMARY KEY,

    ModelId BIGINT NOT NULL,

    PVOfForecastCashFlow DECIMAL(24,4) NULL,

    TerminalValue DECIMAL(24,4) NULL,

    PVOfTerminalValue DECIMAL(24,4) NULL,

    EnterpriseValue DECIMAL(24,4) NULL,

    NetDebt DECIMAL(24,4) NULL,

    EquityValue DECIMAL(24,4) NULL,

    ShareCount DECIMAL(24,6) NULL,

    IntrinsicValuePerShare DECIMAL(18,8) NULL,

    CurrentPrice DECIMAL(18,8) NULL,

    SafetyMargin DECIMAL(18,8) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Valuation_DCFResult_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Valuation_DCFResult_Model
        FOREIGN KEY (ModelId)
        REFERENCES Valuation.DCFModel(ModelId),

    CONSTRAINT UQ_Valuation_DCFResult_Model
        UNIQUE (ModelId)
);
GO
/*
====================================================
 Valuation.IntrinsicValue
 企业内在价值综合结果
====================================================
*/

CREATE TABLE Valuation.IntrinsicValue
(
    ValueId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Valuation_IntrinsicValue PRIMARY KEY,

    StockId INT NOT NULL,

    CalcDate DATE NOT NULL,

    DCFValuePerShare DECIMAL(18,8) NULL,

    GrahamValuePerShare DECIMAL(18,8) NULL,

    EarningsMultipleValue DECIMAL(18,8) NULL,

    DividendValue DECIMAL(18,8) NULL,

    ConservativeValue DECIMAL(18,8) NULL,

    BaseValue DECIMAL(18,8) NULL,

    OptimisticValue DECIMAL(18,8) NULL,

    CurrentPrice DECIMAL(18,8) NULL,

    ConservativeMargin DECIMAL(18,8) NULL,

    BaseMargin DECIMAL(18,8) NULL,

    OptimisticMargin DECIMAL(18,8) NULL,

    ValuationRating VARCHAR(20) NULL,

    ModelVersion VARCHAR(30) NOT NULL,

    DataAsOfDate DATE NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Valuation_IntrinsicValue_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Valuation_IntrinsicValue_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT UQ_Valuation_IntrinsicValue
        UNIQUE
        (
            StockId,
            CalcDate,
            ModelVersion
        )
);
GO
/*
====================================================
 Valuation.ValuationScore
 估值评分
====================================================
*/

CREATE TABLE Valuation.ValuationScore
(
    ScoreId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Valuation_ValuationScore PRIMARY KEY,

    StockId INT NOT NULL,

    ScoreDate DATE NOT NULL,

    PEScore DECIMAL(10,4) NULL,

    PBScore DECIMAL(10,4) NULL,

    PSScore DECIMAL(10,4) NULL,

    DividendYieldScore DECIMAL(10,4) NULL,

    HistoricalPercentileScore DECIMAL(10,4) NULL,

    GrahamScore DECIMAL(10,4) NULL,

    DCFScore DECIMAL(10,4) NULL,

    SafetyMarginScore DECIMAL(10,4) NULL,

    TotalScore DECIMAL(10,4) NOT NULL,

    Grade VARCHAR(10) NULL,

    ModelVersion VARCHAR(30) NOT NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Valuation_ValuationScore_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Valuation_ValuationScore_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT UQ_Valuation_ValuationScore
        UNIQUE
        (
            StockId,
            ScoreDate,
            ModelVersion
        )
);
GO
