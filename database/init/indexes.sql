/*

股票代码查询
    ↓
Stock

股票历史行情
    ↓
StockId + TradeDate

财报
    ↓
StockId + ReportPeriod + PublishDate

评分
    ↓
ModelId + ScoreDate + FinalScore

回测
    ↓
RunId + TradeDate

真实持仓
    ↓
AccountId + StockId

*/
USE AStockQuant;
GO

CREATE INDEX IX_Basic_Stock_Exchange
ON Basic.Stock
(
    ExchangeId,
    IsActive
)
INCLUDE
(
    StockCode,
    StockName,
    ListingDate
);
GO
CREATE INDEX IX_Basic_StockIndustry_Stock
ON Basic.StockIndustry
(
    StockId,
    EffectiveDate,
    ExpireDate
)
INCLUDE
(
    IndustryId,
    IsPrimary
);
GO

CREATE INDEX IX_Basic_StockIndustry_Industry
ON Basic.StockIndustry
(
    IndustryId,
    EffectiveDate,
    ExpireDate
)
INCLUDE
(
    StockId,
    IsPrimary
);
GO
CREATE INDEX IX_Market_StockDailyPrice_TradeDate_Stock
ON Market.StockDailyPrice
(
    TradeDate,
    StockId
)
INCLUDE
(
    ClosePrice,
    ChangePercent,
    Volume,
    Amount,
    TotalMarketCap,
    FloatMarketCap
);
GO
CREATE INDEX IX_Market_StockRealtimeQuote_TradeDate
ON Market.StockRealtimeQuote
(
    TradeDate,
    StockId
)
INCLUDE
(
    CurrentPrice,
    ChangePercent,
    Volume,
    Amount
);
GO
CREATE INDEX IX_Market_IndexDailyPrice_Date
ON Market.IndexDailyPrice
(
    TradeDate,
    IndexId
)
INCLUDE
(
    ClosePrice,
    ChangePercent,
    Volume,
    Amount
);
GO
CREATE INDEX IX_Finance_FinancialReport_Stock_Period
ON Finance.FinancialReport
(
    StockId,
    ReportPeriod DESC
)
INCLUDE
(
    ReportType,
    PublishDate,
    AnnouncementDate,
    IsRestated,
    VersionNo
);
GO
CREATE INDEX IX_Finance_FinancialReport_PublishDate
ON Finance.FinancialReport
(
    PublishDate,
    StockId
)
INCLUDE
(
    ReportPeriod,
    ReportType,
    VersionNo
);
GO
CREATE INDEX IX_Finance_FinancialIndicator_Stock
ON Finance.FinancialIndicator
(
    StockId,
    ReportId
)
INCLUDE
(
    ROE,
    ROIC,
    GrossMargin,
    NetMargin,
    RevenueGrowth,
    NetProfitGrowth,
    FreeCashFlow
);
GO
CREATE INDEX IX_Finance_FinancialMetricValue_Stock_Metric
ON Finance.FinancialMetricValue
(
    StockId,
    MetricId,
    ReportId
)
INCLUDE
(
    MetricValue
);
GO
CREATE INDEX IX_Finance_FinancialMetricValue_Metric_Report
ON Finance.FinancialMetricValue
(
    MetricId,
    ReportId,
    StockId
)
INCLUDE
(
    MetricValue
);
GO
CREATE INDEX IX_Finance_Dividend_Stock_Date
ON Finance.Dividend
(
    StockId,
    ExDividendDate DESC
)
INCLUDE
(
    PaymentDate,
    CashDividendPerShare,
    AfterTaxDividendPerShare
);
GO
CREATE INDEX IX_Buffett_StockIndicatorValue_Stock_Date
ON Buffett.StockIndicatorValue
(
    StockId,
    CalcDate DESC
)
INCLUDE
(
    IndicatorId,
    IndicatorValue,
    Score,
    DataAsOfDate
);
GO
CREATE INDEX IX_Buffett_StockIndicatorValue_Indicator_Date
ON Buffett.StockIndicatorValue
(
    IndicatorId,
    CalcDate DESC,
    StockId
)
INCLUDE
(
    IndicatorValue,
    Score
);
GO
CREATE INDEX IX_Buffett_StockScore_Date
ON Buffett.StockScore
(
    ScoreDate DESC,
    TotalScore DESC
)
INCLUDE
(
    StockId,
    Grade,
    ModelVersion
);
GO
CREATE INDEX IX_Graham_StockIndicatorValue_Stock_Date
ON Graham.StockIndicatorValue
(
    StockId,
    CalcDate DESC
)
INCLUDE
(
    IndicatorId,
    IndicatorValue,
    Score,
    DataAsOfDate
);
GO

CREATE INDEX IX_Graham_StockScore_Date
ON Graham.StockScore
(
    ScoreDate DESC,
    TotalScore DESC
)
INCLUDE
(
    StockId,
    Grade,
    ModelVersion
);
GO
CREATE INDEX IX_Fisher_StockIndicatorValue_Stock_Date
ON Fisher.StockIndicatorValue
(
    StockId,
    CalcDate DESC
)
INCLUDE
(
    IndicatorId,
    IndicatorValue,
    Score,
    DataAsOfDate
);
GO

CREATE INDEX IX_Fisher_StockScore_Date
ON Fisher.StockScore
(
    ScoreDate DESC,
    TotalScore DESC
)
INCLUDE
(
    StockId,
    Grade,
    ModelVersion
);
GO
CREATE INDEX IX_Valuation_MarketValue_Date
ON Valuation.MarketValue
(
    TradeDate,
    StockId
)
INCLUDE
(
    MarketCap,
    CurrentPrice,
    FloatMarketCap
);
GO
CREATE INDEX IX_Valuation_PERatio_Stock_Date
ON Valuation.PERatio
(
    StockId,
    TradeDate DESC
)
INCLUDE
(
    PE,
    PETTM,
    PEForward,
    EarningsPerShare
);
GO
CREATE INDEX IX_Valuation_PBRatio_Stock_Date
ON Valuation.PBRatio
(
    StockId,
    TradeDate DESC
)
INCLUDE
(
    PB,
    BookValuePerShare
);
GO
CREATE INDEX IX_Valuation_PSRatio_Stock_Date
ON Valuation.PSRatio
(
    StockId,
    TradeDate DESC
)
INCLUDE
(
    PS,
    SalesPerShare
);
GO
CREATE INDEX IX_Valuation_HistoricalPercentile_Date
ON Valuation.HistoricalPercentile
(
    CalcDate DESC,
    MetricCode,
    Percentile5Y
)
INCLUDE
(
    StockId,
    CurrentValue,
    Percentile1Y,
    Percentile3Y,
    Percentile10Y,
    MedianValue,
    ModelVersion
);
GO
CREATE INDEX IX_Valuation_DCFModel_Stock_Date
ON Valuation.DCFModel
(
    StockId,
    CalcDate DESC
)
INCLUDE
(
    BaseFreeCashFlow,
    GrowthRate,
    DiscountRate,
    TerminalGrowthRate,
    ModelVersion
);
GO
CREATE INDEX IX_Valuation_DCFProjection_Model
ON Valuation.DCFProjection
(
    ModelId,
    YearNumber
)
INCLUDE
(
    FreeCashFlow,
    DiscountFactor,
    PresentValue
);
GO
CREATE INDEX IX_Valuation_IntrinsicValue_Date
ON Valuation.IntrinsicValue
(
    CalcDate DESC,
    BaseMargin DESC
)
INCLUDE
(
    StockId,
    ConservativeValue,
    BaseValue,
    OptimisticValue,
    CurrentPrice,
    ValuationRating,
    ModelVersion
);
GO
CREATE INDEX IX_Industry_StockIndustryScore_Industry_Date
ON Industry.StockIndustryScore
(
    IndustryId,
    IndicatorId,
    CalcDate DESC,
    RankNo
)
INCLUDE
(
    StockId,
    MetricValue,
    Percentile,
    Score
);
GO
CREATE INDEX IX_Industry_StockIndustryScore_Stock_Date
ON Industry.StockIndustryScore
(
    StockId,
    CalcDate DESC
)
INCLUDE
(
    IndustryId,
    IndicatorId,
    MetricValue,
    RankNo,
    Percentile,
    Score
);
GO
CREATE INDEX IX_Risk_StockRiskIndicator_Date
ON Risk.StockRiskIndicator
(
    CalcDate DESC,
    TotalRiskScore
)
INCLUDE
(
    StockId,
    Volatility30D,
    MaxDrawdown1Y,
    FinancialRiskScore,
    ValuationRiskScore,
    RiskLevel
);
GO
CREATE INDEX IX_Risk_CycleIndicator_Stock_Date
ON Risk.CycleIndicator
(
    StockId,
    CalcDate DESC
)
INCLUDE
(
    CurrentPhase,
    CycleScore,
    ProfitCycleTrend,
    SupplyDemandTrend
);
GO
CREATE INDEX IX_Quant_ScoreModel_Active
ON Quant.ScoreModel
(
    IsActive,
    ModelCode
)
INCLUDE
(
    ModelName,
    Version
);
GO
CREATE INDEX IX_Quant_InvestmentScore_Date_Score
ON Quant.InvestmentScore
(
    ScoreDate DESC,
    ModelId,
    FinalScore DESC
)
INCLUDE
(
    StockId,
    BuffettScore,
    GrahamScore,
    FisherScore,
    ValuationScore,
    IndustryScore,
    RiskAdjustment,
    Rating,
    InvestmentSignal
);
GO
CREATE INDEX IX_Strategy_StockPoolItem_Stock
ON Strategy.StockPoolItem
(
    StockId,
    AddDate DESC
)
INCLUDE
(
    PoolId,
    RemoveDate,
    EntryScore,
    EntryPrice
);
GO
CREATE INDEX IX_Strategy_BacktestRun_Strategy_Date
ON Strategy.BacktestRun
(
    StrategyId,
    StartDate,
    EndDate
)
INCLUDE
(
    TotalReturn,
    AnnualizedReturn,
    MaxDrawdown,
    SharpeRatio,
    Status
);
GO
CREATE INDEX IX_Strategy_BacktestTrade_Run_Date
ON Strategy.BacktestTrade
(
    RunId,
    TradeDate,
    StockId
)
INCLUDE
(
    TradeType,
    Quantity,
    Price,
    Amount,
    Score
);
GO
CREATE INDEX IX_Portfolio_Position_Account
ON Portfolio.Position
(
    AccountId
)
INCLUDE
(
    StockId,
    Quantity,
    AverageCost,
    MarketValue,
    UnrealizedProfitLoss
);
GO
CREATE INDEX IX_Portfolio_Transaction_Account_Date
ON Portfolio.[TradeTransaction]
(
    AccountId,
    TradeDate DESC
)
INCLUDE
(
    StockId,
    TransactionType,
    Quantity,
    Price,
    Amount,
    NetAmount
);
GO
CREATE INDEX IX_Portfolio_Transaction_Stock_Date
ON Portfolio.[TradeTransaction]
(
    StockId,
    TradeDate DESC
)
INCLUDE
(
    AccountId,
    TransactionType,
    Quantity,
    Price,
    Amount
);
GO
CREATE INDEX IX_Portfolio_Snapshot_Date
ON Portfolio.PortfolioSnapshot
(
    SnapshotDate DESC,
    AccountId
)
INCLUDE
(
    TotalAssets,
    TotalReturn,
    DailyReturn,
    Drawdown
);
GO
CREATE INDEX IX_System_DataImportLog_Task_StartTime
ON System.DataImportLog
(
    TaskId,
    StartTime DESC
)
INCLUDE
(
    Status,
    SuccessCount,
    FailedCount,
    InsertedCount,
    UpdatedCount,
    ErrorMessage
);
GO
CREATE INDEX IX_System_DataImportLog_DataType_StartTime
ON System.DataImportLog
(
    DataType,
    StartTime DESC
)
INCLUDE
(
    Status,
    SuccessCount,
    FailedCount
);
GO
