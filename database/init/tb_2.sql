/*
====================================================
 Finance.FinancialReport
 财务报告主表

 Basic.Stock
      │
      ↓
Finance.FinancialReport
      │
      ├──────── IncomeStatement
      │
      ├──────── BalanceSheet
      │
      └──────── CashFlowStatement
                   │
                   ↓
          FinancialIndicator
                   │
                   ↓
           Buffett/Graham/Fisher

分红独立
 Basic.Stock
      │
      ↓
Finance.Dividend
      │
      ├── Graham
      └── Valuation
====================================================
*/

USE AStockQuant;
GO

CREATE TABLE Finance.FinancialReport
(
    ReportId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Finance_FinancialReport PRIMARY KEY,

    StockId INT NOT NULL,

    ReportPeriod DATE NOT NULL,

    ReportType VARCHAR(20) NOT NULL,
    /*
        Q1
        Q2
        Q3
        YEAR
    */

    PublishDate DATE NULL,

    AnnouncementDate DATE NULL,

    AccountingStandard VARCHAR(50) NULL,

    IsRestated BIT NOT NULL
        CONSTRAINT DF_Finance_FinancialReport_IsRestated
        DEFAULT (0),

    VersionNo INT NOT NULL
        CONSTRAINT DF_Finance_FinancialReport_VersionNo
        DEFAULT (1),

    Source VARCHAR(50) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Finance_FinancialReport_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    UpdatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Finance_FinancialReport_UpdatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Finance_FinancialReport_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT CK_Finance_FinancialReport_Type
        CHECK
        (
            ReportType IN
            (
                'Q1',
                'Q2',
                'Q3',
                'YEAR'
            )
        ),

    CONSTRAINT CK_Finance_FinancialReport_Version
        CHECK (VersionNo > 0)
);
GO
/*
====================================================
 Finance.IncomeStatement
 利润表
====================================================
*/

CREATE TABLE Finance.IncomeStatement
(
    ReportId BIGINT
        CONSTRAINT PK_Finance_IncomeStatement PRIMARY KEY,

    Revenue DECIMAL(24,4) NULL,

    OperatingCost DECIMAL(24,4) NULL,

    GrossProfit DECIMAL(24,4) NULL,

    OperatingProfit DECIMAL(24,4) NULL,

    TotalProfit DECIMAL(24,4) NULL,

    NetProfit DECIMAL(24,4) NULL,

    NetProfitAttributable DECIMAL(24,4) NULL,

    NetProfitDeducted DECIMAL(24,4) NULL,

    EPS DECIMAL(18,6) NULL,

    BasicEPS DECIMAL(18,6) NULL,

    DilutedEPS DECIMAL(18,6) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Finance_IncomeStatement_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Finance_IncomeStatement_Report
        FOREIGN KEY (ReportId)
        REFERENCES Finance.FinancialReport(ReportId)
);
GO
/*
====================================================
 Finance.BalanceSheet
 资产负债表
====================================================
*/

CREATE TABLE Finance.BalanceSheet
(
    ReportId BIGINT
        CONSTRAINT PK_Finance_BalanceSheet PRIMARY KEY,

    TotalAssets DECIMAL(24,4) NULL,

    TotalLiabilities DECIMAL(24,4) NULL,

    TotalEquity DECIMAL(24,4) NULL,

    EquityAttributable DECIMAL(24,4) NULL,

    CurrentAssets DECIMAL(24,4) NULL,

    CurrentLiabilities DECIMAL(24,4) NULL,

    CashAndEquivalents DECIMAL(24,4) NULL,

    AccountsReceivable DECIMAL(24,4) NULL,

    Inventory DECIMAL(24,4) NULL,

    FixedAssets DECIMAL(24,4) NULL,

    IntangibleAssets DECIMAL(24,4) NULL,

    Goodwill DECIMAL(24,4) NULL,

    ShortTermDebt DECIMAL(24,4) NULL,

    LongTermDebt DECIMAL(24,4) NULL,

    InterestBearingDebt DECIMAL(24,4) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Finance_BalanceSheet_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Finance_BalanceSheet_Report
        FOREIGN KEY (ReportId)
        REFERENCES Finance.FinancialReport(ReportId)
);
GO
/*
====================================================
 Finance.CashFlowStatement
 现金流量表
====================================================
*/

CREATE TABLE Finance.CashFlowStatement
(
    ReportId BIGINT
        CONSTRAINT PK_Finance_CashFlowStatement PRIMARY KEY,

    OperatingCashFlow DECIMAL(24,4) NULL,

    InvestingCashFlow DECIMAL(24,4) NULL,

    FinancingCashFlow DECIMAL(24,4) NULL,

    CapitalExpenditure DECIMAL(24,4) NULL,

    CashFromCustomers DECIMAL(24,4) NULL,

    CashToSuppliers DECIMAL(24,4) NULL,

    CashTaxesPaid DECIMAL(24,4) NULL,

    CashInterestPaid DECIMAL(24,4) NULL,

    NetCashChange DECIMAL(24,4) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Finance_CashFlowStatement_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Finance_CashFlowStatement_Report
        FOREIGN KEY (ReportId)
        REFERENCES Finance.FinancialReport(ReportId)
);
GO
/*
====================================================
 Finance.FinancialIndicator
 标准财务指标
====================================================
*/

CREATE TABLE Finance.FinancialIndicator
(
    IndicatorId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Finance_FinancialIndicator PRIMARY KEY,

    StockId INT NOT NULL,

    ReportId BIGINT NOT NULL,

    ROE DECIMAL(18,6) NULL,

    ROA DECIMAL(18,6) NULL,

    ROIC DECIMAL(18,6) NULL,

    GrossMargin DECIMAL(18,6) NULL,

    OperatingMargin DECIMAL(18,6) NULL,

    NetMargin DECIMAL(18,6) NULL,

    DebtRatio DECIMAL(18,6) NULL,

    CurrentRatio DECIMAL(18,6) NULL,

    QuickRatio DECIMAL(18,6) NULL,

    RevenueGrowth DECIMAL(18,6) NULL,

    NetProfitGrowth DECIMAL(18,6) NULL,

    OperatingCashFlowGrowth DECIMAL(18,6) NULL,

    OperatingCashFlowToNetProfit DECIMAL(18,6) NULL,

    FreeCashFlow DECIMAL(24,4) NULL,

    FreeCashFlowMargin DECIMAL(18,6) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Finance_FinancialIndicator_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Finance_FinancialIndicator_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT FK_Finance_FinancialIndicator_Report
        FOREIGN KEY (ReportId)
        REFERENCES Finance.FinancialReport(ReportId),

    CONSTRAINT UQ_Finance_FinancialIndicator_Report
        UNIQUE (ReportId)
);
GO
/*
====================================================
 Finance.Dividend
 分红记录
====================================================
*/

CREATE TABLE Finance.Dividend
(
    DividendId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Finance_Dividend PRIMARY KEY,

    StockId INT NOT NULL,

    ExDividendDate DATE NULL,

    RecordDate DATE NULL,

    PaymentDate DATE NULL,

    AnnouncementDate DATE NULL,

    CashDividendPerShare DECIMAL(18,6) NULL,

    DividendAmount DECIMAL(24,4) NULL,

    BeforeTaxDividendPerShare DECIMAL(18,6) NULL,

    AfterTaxDividendPerShare DECIMAL(18,6) NULL,

    IsFinalDividend BIT NOT NULL
        CONSTRAINT DF_Finance_Dividend_IsFinalDividend
        DEFAULT (1),

    Source VARCHAR(50) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Finance_Dividend_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Finance_Dividend_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId)
);
GO
/*
====================================================
 Finance.FinancialMetricDefinition
 财务指标定义
====================================================
*/

CREATE TABLE Finance.FinancialMetricDefinition
(
    MetricId INT IDENTITY(1,1)
        CONSTRAINT PK_Finance_FinancialMetricDefinition
        PRIMARY KEY,

    MetricCode VARCHAR(50) NOT NULL,

    MetricName NVARCHAR(100) NOT NULL,

    Unit VARCHAR(30) NULL,

    Description NVARCHAR(500) NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Finance_FinancialMetricDefinition_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Finance_FinancialMetricDefinition_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Finance_FinancialMetricDefinition_Code
        UNIQUE (MetricCode)
);
GO
/*
====================================================
 Finance.FinancialMetricValue
 财务指标历史值
====================================================
*/

CREATE TABLE Finance.FinancialMetricValue
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Finance_FinancialMetricValue
        PRIMARY KEY,

    StockId INT NOT NULL,

    ReportId BIGINT NOT NULL,

    MetricId INT NOT NULL,

    MetricValue DECIMAL(24,8) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Finance_FinancialMetricValue_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Finance_FinancialMetricValue_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT FK_Finance_FinancialMetricValue_Report
        FOREIGN KEY (ReportId)
        REFERENCES Finance.FinancialReport(ReportId),

    CONSTRAINT FK_Finance_FinancialMetricValue_Metric
        FOREIGN KEY (MetricId)
        REFERENCES Finance.FinancialMetricDefinition(MetricId),

    CONSTRAINT UQ_Finance_FinancialMetricValue
        UNIQUE
        (
            StockId,
            ReportId,
            MetricId
        )
);
GO
