/*
====================================================
 Industry.Indicator
 行业分析指标定义

 三大财报
   ↓
Finance
   ↓
Buffett / Graham / Fisher
   ↓
Valuation
   ↓
Industry
   ↓
Risk
   ↓
Quant.ScoreModel
   ↓
Quant.InvestmentScore
   ↓
Strategy
   ↓
Backtest

====================================================
*/

USE AStockQuant;
GO

CREATE TABLE Industry.Indicator
(
    IndicatorId INT IDENTITY(1,1)
        CONSTRAINT PK_Industry_Indicator PRIMARY KEY,

    IndicatorCode VARCHAR(20) NOT NULL,

    IndicatorName NVARCHAR(100) NOT NULL,

    Unit VARCHAR(30) NULL,

    Description NVARCHAR(1000) NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Industry_Indicator_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Industry_Indicator_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Industry_Indicator_Code
        UNIQUE (IndicatorCode)
);
GO
/*
====================================================
 Industry.StockIndustryScore
 股票行业比较结果
====================================================
*/

CREATE TABLE Industry.StockIndustryScore
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Industry_StockIndustryScore PRIMARY KEY,

    StockId INT NOT NULL,

    IndustryId INT NOT NULL,

    IndicatorId INT NOT NULL,

    CalcDate DATE NOT NULL,

    MetricValue DECIMAL(24,8) NULL,

    RankNo INT NULL,

    TotalCompanies INT NULL,

    Percentile DECIMAL(10,6) NULL,

    Score DECIMAL(10,4) NULL,

    DataAsOfDate DATE NULL,

    ModelVersion VARCHAR(30) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Industry_StockIndustryScore_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Industry_StockIndustryScore_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT FK_Industry_StockIndustryScore_Industry
        FOREIGN KEY (IndustryId)
        REFERENCES Basic.Industry(IndustryId),

    CONSTRAINT FK_Industry_StockIndustryScore_Indicator
        FOREIGN KEY (IndicatorId)
        REFERENCES Industry.Indicator(IndicatorId),

    CONSTRAINT UQ_Industry_StockIndustryScore
        UNIQUE
        (
            StockId,
            IndustryId,
            IndicatorId,
            CalcDate
        )
);
GO
/*
====================================================
 Risk.StockRiskIndicator
 股票风险指标
====================================================
*/

CREATE TABLE Risk.StockRiskIndicator
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Risk_StockRiskIndicator PRIMARY KEY,

    StockId INT NOT NULL,

    CalcDate DATE NOT NULL,

    Volatility30D DECIMAL(18,8) NULL,

    Volatility60D DECIMAL(18,8) NULL,

    Volatility252D DECIMAL(18,8) NULL,

    MaxDrawdown1Y DECIMAL(18,8) NULL,

    MaxDrawdown3Y DECIMAL(18,8) NULL,

    Beta DECIMAL(18,8) NULL,

    DebtRiskScore DECIMAL(10,4) NULL,

    LiquidityRiskScore DECIMAL(10,4) NULL,

    ValuationRiskScore DECIMAL(10,4) NULL,

    FinancialRiskScore DECIMAL(10,4) NULL,

    TotalRiskScore DECIMAL(10,4) NULL,

    RiskLevel VARCHAR(20) NULL,

    DataAsOfDate DATE NULL,

    ModelVersion VARCHAR(30) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Risk_StockRiskIndicator_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Risk_StockRiskIndicator_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT UQ_Risk_StockRiskIndicator
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
 Risk.CycleIndicator
 周期行业状态
====================================================
*/

CREATE TABLE Risk.CycleIndicator
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Risk_CycleIndicator PRIMARY KEY,

    StockId INT NOT NULL,

    CalcDate DATE NOT NULL,

    CycleType VARCHAR(50) NOT NULL,

    CurrentPhase VARCHAR(50) NULL,

    CycleScore DECIMAL(10,4) NULL,

    CommodityPriceTrend DECIMAL(18,8) NULL,

    ProfitCycleTrend DECIMAL(18,8) NULL,

    InventoryTrend DECIMAL(18,8) NULL,

    SupplyDemandTrend DECIMAL(18,8) NULL,

    Description NVARCHAR(1000) NULL,

    ModelVersion VARCHAR(30) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Risk_CycleIndicator_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Risk_CycleIndicator_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT UQ_Risk_CycleIndicator
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
 Quant.ScoreModel
 综合评分模型定义
====================================================
*/

CREATE TABLE Quant.ScoreModel
(
    ModelId INT IDENTITY(1,1)
        CONSTRAINT PK_Quant_ScoreModel PRIMARY KEY,

    ModelCode VARCHAR(50) NOT NULL,

    ModelName NVARCHAR(100) NOT NULL,

    Version VARCHAR(30) NOT NULL,

    Description NVARCHAR(1000) NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Quant_ScoreModel_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Quant_ScoreModel_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Quant_ScoreModel
        UNIQUE (ModelCode, Version)
);
GO
/*
====================================================
 Quant.ScoreModelWeight
 综合模型权重
====================================================
*/

CREATE TABLE Quant.ScoreModelWeight
(
    Id INT IDENTITY(1,1)
        CONSTRAINT PK_Quant_ScoreModelWeight PRIMARY KEY,

    ModelId INT NOT NULL,

    ComponentCode VARCHAR(30) NOT NULL,

    Weight DECIMAL(10,6) NOT NULL,

    MaxScore DECIMAL(10,4) NOT NULL,

    Description NVARCHAR(500) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Quant_ScoreModelWeight_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Quant_ScoreModelWeight_Model
        FOREIGN KEY (ModelId)
        REFERENCES Quant.ScoreModel(ModelId),

    CONSTRAINT UQ_Quant_ScoreModelWeight
        UNIQUE (ModelId, ComponentCode),

    CONSTRAINT CK_Quant_ScoreModelWeight_Weight
        CHECK (Weight >= 0),

    CONSTRAINT CK_Quant_ScoreModelWeight_MaxScore
        CHECK (MaxScore > 0)
);
GO
/*
====================================================
 Quant.InvestmentScore
 股票最终投资评分
====================================================
*/

CREATE TABLE Quant.InvestmentScore
(
    ScoreId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Quant_InvestmentScore PRIMARY KEY,

    StockId INT NOT NULL,

    ModelId INT NOT NULL,

    ScoreDate DATE NOT NULL,

    BuffettScore DECIMAL(10,4) NULL,

    GrahamScore DECIMAL(10,4) NULL,

    FisherScore DECIMAL(10,4) NULL,

    ValuationScore DECIMAL(10,4) NULL,

    IndustryScore DECIMAL(10,4) NULL,

    RiskAdjustment DECIMAL(10,6) NULL,

    BaseScore DECIMAL(10,4) NULL,

    FinalScore DECIMAL(10,4) NOT NULL,

    Rating VARCHAR(20) NULL,

    InvestmentSignal VARCHAR(30) NULL,

    DataAsOfDate DATE NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Quant_InvestmentScore_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Quant_InvestmentScore_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT FK_Quant_InvestmentScore_Model
        FOREIGN KEY (ModelId)
        REFERENCES Quant.ScoreModel(ModelId),

    CONSTRAINT UQ_Quant_InvestmentScore
        UNIQUE
        (
            StockId,
            ModelId,
            ScoreDate
        )
);
GO
/*
====================================================
 Strategy.StockPool
 股票池
====================================================
*/

CREATE TABLE Strategy.StockPool
(
    PoolId INT IDENTITY(1,1)
        CONSTRAINT PK_Strategy_StockPool PRIMARY KEY,

    PoolCode VARCHAR(50) NOT NULL,

    PoolName NVARCHAR(100) NOT NULL,

    Description NVARCHAR(1000) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Strategy_StockPool_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Strategy_StockPool_Code
        UNIQUE (PoolCode)
);
GO
/*
====================================================
 Strategy.StockPoolItem
 股票池成员
====================================================
*/

CREATE TABLE Strategy.StockPoolItem
(
    PoolId INT NOT NULL,

    StockId INT NOT NULL,

    AddDate DATE NOT NULL,

    RemoveDate DATE NULL,

    EntryScore DECIMAL(10,4) NULL,

    EntryPrice DECIMAL(18,6) NULL,

    Reason NVARCHAR(1000) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Strategy_StockPoolItem_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Strategy_StockPoolItem
        PRIMARY KEY
        (
            PoolId,
            StockId,
            AddDate
        ),

    CONSTRAINT FK_Strategy_StockPoolItem_Pool
        FOREIGN KEY (PoolId)
        REFERENCES Strategy.StockPool(PoolId),

    CONSTRAINT FK_Strategy_StockPoolItem_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT CK_Strategy_StockPoolItem_Dates
        CHECK
        (
            RemoveDate IS NULL
            OR RemoveDate >= AddDate
        )
);
GO
/*
====================================================
 Strategy.BacktestStrategy
 回测策略定义
====================================================
*/

CREATE TABLE Strategy.BacktestStrategy
(
    StrategyId INT IDENTITY(1,1)
        CONSTRAINT PK_Strategy_BacktestStrategy PRIMARY KEY,

    StrategyCode VARCHAR(50) NOT NULL,

    StrategyName NVARCHAR(100) NOT NULL,

    ScoreModelId INT NULL,

    Description NVARCHAR(2000) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Strategy_BacktestStrategy_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Strategy_BacktestStrategy_Code
        UNIQUE (StrategyCode),

    CONSTRAINT FK_Strategy_BacktestStrategy_Model
        FOREIGN KEY (ScoreModelId)
        REFERENCES Quant.ScoreModel(ModelId)
);
GO
/*
====================================================
 Strategy.BacktestRun
 回测运行
====================================================
*/

CREATE TABLE Strategy.BacktestRun
(
    RunId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Strategy_BacktestRun PRIMARY KEY,

    StrategyId INT NOT NULL,

    StartDate DATE NOT NULL,

    EndDate DATE NOT NULL,

    InitialCapital DECIMAL(24,4) NOT NULL,

    FinalCapital DECIMAL(24,4) NULL,

    TotalReturn DECIMAL(18,8) NULL,

    AnnualizedReturn DECIMAL(18,8) NULL,

    MaxDrawdown DECIMAL(18,8) NULL,

    SharpeRatio DECIMAL(18,8) NULL,

    WinRate DECIMAL(18,8) NULL,

    TotalTrades INT NULL,

    Status VARCHAR(20) NOT NULL
        CONSTRAINT DF_Strategy_BacktestRun_Status
        DEFAULT ('Pending'),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Strategy_BacktestRun_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CompletedTime DATETIME2(0) NULL,

    CONSTRAINT FK_Strategy_BacktestRun_Strategy
        FOREIGN KEY (StrategyId)
        REFERENCES Strategy.BacktestStrategy(StrategyId),

    CONSTRAINT CK_Strategy_BacktestRun_Dates
        CHECK (EndDate >= StartDate),

    CONSTRAINT CK_Strategy_BacktestRun_Capital
        CHECK (InitialCapital > 0)
);
GO
/*
====================================================
 Strategy.BacktestTrade
 回测交易记录
====================================================
*/

CREATE TABLE Strategy.BacktestTrade
(
    TradeId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Strategy_BacktestTrade PRIMARY KEY,

    RunId BIGINT NOT NULL,

    StockId INT NOT NULL,

    TradeDate DATE NOT NULL,

    TradeType VARCHAR(20) NOT NULL,

    Quantity INT NOT NULL,

    Price DECIMAL(18,6) NOT NULL,

    Amount DECIMAL(24,4) NOT NULL,

    Commission DECIMAL(24,4) NULL,

    Slippage DECIMAL(18,8) NULL,

    Score DECIMAL(10,4) NULL,

    Reason NVARCHAR(1000) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Strategy_BacktestTrade_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Strategy_BacktestTrade_Run
        FOREIGN KEY (RunId)
        REFERENCES Strategy.BacktestRun(RunId),

    CONSTRAINT FK_Strategy_BacktestTrade_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT CK_Strategy_BacktestTrade_Type
        CHECK
        (
            TradeType IN ('BUY', 'SELL')
        ),

    CONSTRAINT CK_Strategy_BacktestTrade_Quantity
        CHECK (Quantity > 0),

    CONSTRAINT CK_Strategy_BacktestTrade_Price
        CHECK (Price >= 0)
);
GO
