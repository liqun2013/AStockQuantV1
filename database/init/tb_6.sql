/*
====================================================
 Portfolio.Account
 投资账户
====================================================
*/

USE AStockQuant;
GO

CREATE TABLE Portfolio.Account
(
    AccountId INT IDENTITY(1,1)
        CONSTRAINT PK_Portfolio_Account PRIMARY KEY,

    AccountCode VARCHAR(50) NOT NULL,

    AccountName NVARCHAR(100) NOT NULL,

    BrokerName NVARCHAR(100) NULL,

    InitialCapital DECIMAL(24,4) NOT NULL,

    Currency VARCHAR(10) NOT NULL
        CONSTRAINT DF_Portfolio_Account_Currency
        DEFAULT ('CNY'),

    IsActive BIT NOT NULL
        CONSTRAINT DF_Portfolio_Account_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Portfolio_Account_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Portfolio_Account_Code
        UNIQUE (AccountCode),

    CONSTRAINT CK_Portfolio_Account_Capital
        CHECK (InitialCapital >= 0)
);
GO
/*
====================================================
 Portfolio.Position
 当前持仓
====================================================
*/

CREATE TABLE Portfolio.Position
(
    PositionId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Portfolio_Position PRIMARY KEY,

    AccountId INT NOT NULL,

    StockId INT NOT NULL,

    Quantity INT NOT NULL,

    AvailableQuantity INT NOT NULL,

    AverageCost DECIMAL(18,6) NOT NULL,

    CurrentPrice DECIMAL(18,6) NULL,

    MarketValue DECIMAL(24,4) NULL,

    CostValue DECIMAL(24,4) NULL,

    UnrealizedProfitLoss DECIMAL(24,4) NULL,

    UnrealizedReturnRate DECIMAL(18,8) NULL,

    DividendIncome DECIMAL(24,4) NULL,

    LastUpdatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Portfolio_Position_LastUpdatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Portfolio_Position_Account
        FOREIGN KEY (AccountId)
        REFERENCES Portfolio.Account(AccountId),

    CONSTRAINT FK_Portfolio_Position_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT UQ_Portfolio_Position
        UNIQUE (AccountId, StockId),

    CONSTRAINT CK_Portfolio_Position_Quantity
        CHECK
        (
            Quantity >= 0
            AND AvailableQuantity >= 0
            AND AvailableQuantity <= Quantity
        ),

    CONSTRAINT CK_Portfolio_Position_Cost
        CHECK (AverageCost >= 0)
);
GO
/*
====================================================
 Portfolio.TradeTransaction
 实际投资交易记录
====================================================
*/

CREATE TABLE Portfolio.[TradeTransaction]
(
    TransactionId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Portfolio_Transaction PRIMARY KEY,

    AccountId INT NOT NULL,

    StockId INT NULL,

    TradeDate DATE NOT NULL,

    TradeTime DATETIME2(0) NULL,

    TransactionType VARCHAR(30) NOT NULL,
    /*
        BUY
        SELL
        DIVIDEND
        DEPOSIT
        WITHDRAW
        FEE
        TAX
    */

    Quantity INT NULL,

    Price DECIMAL(18,6) NULL,

    Amount DECIMAL(24,4) NOT NULL,

    Commission DECIMAL(24,4) NULL,

    Tax DECIMAL(24,4) NULL,

    NetAmount DECIMAL(24,4) NULL,

    OrderNo VARCHAR(100) NULL,

    Reason NVARCHAR(1000) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Portfolio_Transaction_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Portfolio_Transaction_Account
        FOREIGN KEY (AccountId)
        REFERENCES Portfolio.Account(AccountId),

    CONSTRAINT FK_Portfolio_Transaction_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT CK_Portfolio_Transaction_Type
        CHECK
        (
            TransactionType IN
            (
                'BUY',
                'SELL',
                'DIVIDEND',
                'DEPOSIT',
                'WITHDRAW',
                'FEE',
                'TAX'
            )
        ),

    CONSTRAINT CK_Portfolio_Transaction_Quantity
        CHECK
        (
            Quantity IS NULL
            OR Quantity >= 0
        ),

    CONSTRAINT CK_Portfolio_Transaction_Price
        CHECK
        (
            Price IS NULL
            OR Price >= 0
        )
);
GO
/*
====================================================
 Portfolio.PortfolioSnapshot
 投资组合每日快照
====================================================
*/

CREATE TABLE Portfolio.PortfolioSnapshot
(
    SnapshotId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Portfolio_PortfolioSnapshot PRIMARY KEY,

    AccountId INT NOT NULL,

    SnapshotDate DATE NOT NULL,

    CashBalance DECIMAL(24,4) NULL,

    MarketValue DECIMAL(24,4) NULL,

    TotalAssets DECIMAL(24,4) NULL,

    TotalCost DECIMAL(24,4) NULL,

    UnrealizedProfitLoss DECIMAL(24,4) NULL,

    RealizedProfitLoss DECIMAL(24,4) NULL,

    DividendIncome DECIMAL(24,4) NULL,

    TotalReturn DECIMAL(18,8) NULL,

    DailyReturn DECIMAL(18,8) NULL,

    Drawdown DECIMAL(18,8) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Portfolio_PortfolioSnapshot_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Portfolio_PortfolioSnapshot_Account
        FOREIGN KEY (AccountId)
        REFERENCES Portfolio.Account(AccountId),

    CONSTRAINT UQ_Portfolio_PortfolioSnapshot
        UNIQUE (AccountId, SnapshotDate)
);
GO
/*
====================================================
 Portfolio.InvestmentReview
 投资复盘
====================================================
*/

CREATE TABLE Portfolio.InvestmentReview
(
    ReviewId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Portfolio_InvestmentReview PRIMARY KEY,

    AccountId INT NULL,

    StockId INT NOT NULL,

    ReviewDate DATE NOT NULL,

    Action VARCHAR(20) NOT NULL,

    InvestmentThesis NVARCHAR(MAX) NULL,

    BuffettView NVARCHAR(MAX) NULL,

    GrahamView NVARCHAR(MAX) NULL,

    FisherView NVARCHAR(MAX) NULL,

    ValuationView NVARCHAR(MAX) NULL,

    RiskView NVARCHAR(MAX) NULL,

    Conclusion NVARCHAR(MAX) NULL,

    ExpectedReturn DECIMAL(18,8) NULL,

    TargetPrice DECIMAL(18,6) NULL,

    StopCondition NVARCHAR(1000) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Portfolio_InvestmentReview_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Portfolio_InvestmentReview_Account
        FOREIGN KEY (AccountId)
        REFERENCES Portfolio.Account(AccountId),

    CONSTRAINT FK_Portfolio_InvestmentReview_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT CK_Portfolio_InvestmentReview_Action
        CHECK
        (
            Action IN
            (
                'BUY',
                'SELL',
                'HOLD',
                'WATCH'
            )
        )
);
GO
/*
====================================================
 Portfolio.DividendRecord
 实际分红到账记录
====================================================
*/

CREATE TABLE Portfolio.DividendRecord
(
    DividendRecordId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Portfolio_DividendRecord PRIMARY KEY,

    AccountId INT NOT NULL,

    StockId INT NOT NULL,

    DividendDate DATE NOT NULL,

    Quantity INT NULL,

    DividendPerShare DECIMAL(18,8) NULL,

    GrossAmount DECIMAL(24,4) NULL,

    TaxAmount DECIMAL(24,4) NULL,

    NetAmount DECIMAL(24,4) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Portfolio_DividendRecord_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Portfolio_DividendRecord_Account
        FOREIGN KEY (AccountId)
        REFERENCES Portfolio.Account(AccountId),

    CONSTRAINT FK_Portfolio_DividendRecord_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId)
);
GO
/*
====================================================
 System.DataSource
 数据源
====================================================
*/

CREATE TABLE System.DataSource
(
    DataSourceId INT IDENTITY(1,1)
        CONSTRAINT PK_System_DataSource PRIMARY KEY,

    SourceCode VARCHAR(50) NOT NULL,

    SourceName NVARCHAR(100) NOT NULL,

    SourceType VARCHAR(30) NOT NULL,
    /*
        AKTOOLS
        AKSHARE
        MANUAL
        OTHER
    */

    BaseUrl VARCHAR(500) NULL,

    IsEnabled BIT NOT NULL
        CONSTRAINT DF_System_DataSource_IsEnabled
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_System_DataSource_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_System_DataSource_Code
        UNIQUE (SourceCode)
);
GO
/*
====================================================
 System.DataImportTask
 数据导入任务
====================================================
*/

CREATE TABLE System.DataImportTask
(
    TaskId INT IDENTITY(1,1)
        CONSTRAINT PK_System_DataImportTask PRIMARY KEY,

    TaskCode VARCHAR(100) NOT NULL,

    TaskName NVARCHAR(200) NOT NULL,

    DataSourceId INT NOT NULL,

    DataType VARCHAR(50) NOT NULL,
    /*
        STOCK_LIST
        REALTIME_QUOTE
        DAILY_PRICE
        FINANCIAL_REPORT
        DIVIDEND
        INDUSTRY
        ...
    */

    CronExpression VARCHAR(100) NULL,

    Enabled BIT NOT NULL
        CONSTRAINT DF_System_DataImportTask_Enabled
        DEFAULT (1),

    RetryCount INT NOT NULL
        CONSTRAINT DF_System_DataImportTask_RetryCount
        DEFAULT (3),

    TimeoutSeconds INT NOT NULL
        CONSTRAINT DF_System_DataImportTask_TimeoutSeconds
        DEFAULT (120),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_System_DataImportTask_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_System_DataImportTask_Code
        UNIQUE (TaskCode),

    CONSTRAINT FK_System_DataImportTask_DataSource
        FOREIGN KEY (DataSourceId)
        REFERENCES System.DataSource(DataSourceId),

    CONSTRAINT CK_System_DataImportTask_RetryCount
        CHECK (RetryCount >= 0),

    CONSTRAINT CK_System_DataImportTask_Timeout
        CHECK (TimeoutSeconds > 0)
);
GO
/*
====================================================
 System.DataImportLog
 数据导入日志
====================================================
*/

CREATE TABLE System.DataImportLog
(
    ImportLogId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_System_DataImportLog PRIMARY KEY,

    TaskId INT NULL,

    DataSourceId INT NULL,

    DataType VARCHAR(50) NOT NULL,

    StartTime DATETIME2(0) NOT NULL,

    EndTime DATETIME2(0) NULL,

    Status VARCHAR(20) NOT NULL,

    RequestCount INT NULL,

    SuccessCount INT NULL,

    FailedCount INT NULL,

    InsertedCount INT NULL,

    UpdatedCount INT NULL,

    SkippedCount INT NULL,

    ErrorMessage NVARCHAR(MAX) NULL,

    ErrorDetail NVARCHAR(MAX) NULL,

    CorrelationId VARCHAR(100) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_System_DataImportLog_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_System_DataImportLog_Task
        FOREIGN KEY (TaskId)
        REFERENCES System.DataImportTask(TaskId),

    CONSTRAINT FK_System_DataImportLog_DataSource
        FOREIGN KEY (DataSourceId)
        REFERENCES System.DataSource(DataSourceId),

    CONSTRAINT CK_System_DataImportLog_Status
        CHECK
        (
            Status IN
            (
                'PENDING',
                'RUNNING',
                'SUCCESS',
                'PARTIAL',
                'FAILED'
            )
        )
);
GO
/*
====================================================
 System.SystemSetting
 系统配置
====================================================
*/

CREATE TABLE System.SystemSetting
(
    SettingId INT IDENTITY(1,1)
        CONSTRAINT PK_System_SystemSetting PRIMARY KEY,

    SettingKey VARCHAR(100) NOT NULL,

    SettingValue NVARCHAR(2000) NULL,

    Description NVARCHAR(500) NULL,

    IsEncrypted BIT NOT NULL
        CONSTRAINT DF_System_SystemSetting_IsEncrypted
        DEFAULT (0),

    UpdatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_System_SystemSetting_UpdatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_System_SystemSetting_Key
        UNIQUE (SettingKey)
);
GO
