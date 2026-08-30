/*
====================================================
 03_CreateTables.sql
 Part 1 - Basic + Market
 SQL Server 2019
====================================================
*/

USE AStockQuant;
GO

/*
====================================================
 Basic.Exchange 交易所

 Basic.Exchange
      │
      ├──────────────┐
      ↓              ↓
Basic.Stock      Market.IndexInfo
      │              │
      │              ↓
      │        IndexDailyPrice
      │
      ├── StockIndustry ── Industry
      │
      ├── StockDailyPrice
      │
      └── StockRealtimeQuote

====================================================
*/

CREATE TABLE Basic.Exchange
(
    ExchangeId INT IDENTITY(1,1)
        CONSTRAINT PK_Basic_Exchange PRIMARY KEY,

    ExchangeCode VARCHAR(20) NOT NULL,

    ExchangeName NVARCHAR(100) NOT NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Basic_Exchange_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Basic_Exchange_Code
        UNIQUE (ExchangeCode)
);
GO
/*
====================================================
 Basic.Industry 行业
====================================================
*/

CREATE TABLE Basic.Industry
(
    IndustryId INT IDENTITY(1,1)
        CONSTRAINT PK_Basic_Industry PRIMARY KEY,

    IndustryCode VARCHAR(50) NOT NULL,

    IndustryName NVARCHAR(100) NOT NULL,

    IndustryLevel TINYINT NOT NULL,

    ParentIndustryId INT NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Basic_Industry_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Basic_Industry_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Basic_Industry_Code
        UNIQUE (IndustryCode),

    CONSTRAINT CK_Basic_Industry_Level
        CHECK (IndustryLevel BETWEEN 1 AND 3),

    CONSTRAINT FK_Basic_Industry_Parent
        FOREIGN KEY (ParentIndustryId)
        REFERENCES Basic.Industry(IndustryId)
);
GO
/*
====================================================
 Basic.Stock 股票
====================================================
*/

CREATE TABLE Basic.Stock
(
    StockId INT IDENTITY(1,1)
        CONSTRAINT PK_Basic_Stock PRIMARY KEY,

    StockCode VARCHAR(20) NOT NULL,

    StockName NVARCHAR(100) NOT NULL,

    ExchangeId INT NOT NULL,

    ListingDate DATE NULL,

    DelistingDate DATE NULL,

    SecurityType VARCHAR(30) NOT NULL
        CONSTRAINT DF_Basic_Stock_SecurityType
        DEFAULT ('Stock'),

    MarketType VARCHAR(30) NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Basic_Stock_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Basic_Stock_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    UpdatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Basic_Stock_UpdatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Basic_Stock_Code
        UNIQUE (StockCode),

    CONSTRAINT FK_Basic_Stock_Exchange
        FOREIGN KEY (ExchangeId)
        REFERENCES Basic.Exchange(ExchangeId),

    CONSTRAINT CK_Basic_Stock_Dates
        CHECK
        (
            DelistingDate IS NULL
            OR ListingDate IS NULL
            OR DelistingDate >= ListingDate
        )
);
GO
/*
====================================================
 Basic.StockIndustry 股票行业关系
====================================================
*/

CREATE TABLE Basic.StockIndustry
(
    StockIndustryId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Basic_StockIndustry PRIMARY KEY,

    StockId INT NOT NULL,

    IndustryId INT NOT NULL,

    EffectiveDate DATE NOT NULL,

    ExpireDate DATE NULL,

    IsPrimary BIT NOT NULL
        CONSTRAINT DF_Basic_StockIndustry_IsPrimary
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Basic_StockIndustry_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Basic_StockIndustry_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT FK_Basic_StockIndustry_Industry
        FOREIGN KEY (IndustryId)
        REFERENCES Basic.Industry(IndustryId),

    CONSTRAINT CK_Basic_StockIndustry_Dates
        CHECK
        (
            ExpireDate IS NULL
            OR ExpireDate >= EffectiveDate
        )
);
GO
/*
====================================================
 Market.StockDailyPrice 股票日行情
====================================================
*/

CREATE TABLE Market.StockDailyPrice
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Market_StockDailyPrice PRIMARY KEY,

    StockId INT NOT NULL,

    TradeDate DATE NOT NULL,

    OpenPrice DECIMAL(18,4) NULL,

    HighPrice DECIMAL(18,4) NULL,

    LowPrice DECIMAL(18,4) NULL,

    ClosePrice DECIMAL(18,4) NULL,

    PrevClosePrice DECIMAL(18,4) NULL,

    ChangeAmount DECIMAL(18,4) NULL,

    ChangePercent DECIMAL(12,6) NULL,

    Volume BIGINT NULL,

    Amount DECIMAL(24,4) NULL,

    TurnoverRate DECIMAL(12,6) NULL,

    TotalMarketCap DECIMAL(24,4) NULL,

    FloatMarketCap DECIMAL(24,4) NULL,

    IsSuspended BIT NOT NULL
        CONSTRAINT DF_Market_StockDailyPrice_IsSuspended
        DEFAULT (0),

    Source VARCHAR(50) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Market_StockDailyPrice_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Market_StockDailyPrice_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT UQ_Market_StockDailyPrice_StockDate
        UNIQUE (StockId, TradeDate)
);
GO
/*
====================================================
 Market.StockRealtimeQuote 实时行情
====================================================
*/

CREATE TABLE Market.StockRealtimeQuote
(
    StockId INT
        CONSTRAINT PK_Market_StockRealtimeQuote PRIMARY KEY,

    TradeDate DATE NULL,

    TradeTime DATETIME2(0) NULL,

    CurrentPrice DECIMAL(18,4) NULL,

    OpenPrice DECIMAL(18,4) NULL,

    HighPrice DECIMAL(18,4) NULL,

    LowPrice DECIMAL(18,4) NULL,

    PrevClosePrice DECIMAL(18,4) NULL,

    ChangeAmount DECIMAL(18,4) NULL,

    ChangePercent DECIMAL(12,6) NULL,

    Volume BIGINT NULL,

    Amount DECIMAL(24,4) NULL,

    TurnoverRate DECIMAL(12,6) NULL,

    TotalMarketCap DECIMAL(24,4) NULL,

    FloatMarketCap DECIMAL(24,4) NULL,

    Source VARCHAR(50) NULL,

    UpdatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Market_StockRealtimeQuote_UpdatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Market_StockRealtimeQuote_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId)
);
GO
/*
====================================================
 Market.IndexInfo 指数行情
====================================================
*/

CREATE TABLE Market.IndexInfo
(
    IndexId INT IDENTITY(1,1)
        CONSTRAINT PK_Market_IndexInfo PRIMARY KEY,

    IndexCode VARCHAR(30) NOT NULL,

    IndexName NVARCHAR(100) NOT NULL,

    ExchangeId INT NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Market_IndexInfo_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Market_IndexInfo_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Market_IndexInfo_Code
        UNIQUE (IndexCode),

    CONSTRAINT FK_Market_IndexInfo_Exchange
        FOREIGN KEY (ExchangeId)
        REFERENCES Basic.Exchange(ExchangeId)
);
GO
/*
====================================================
 Market.IndexDailyPrice 指数日行情
====================================================
*/

CREATE TABLE Market.IndexDailyPrice
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Market_IndexDailyPrice PRIMARY KEY,

    IndexId INT NOT NULL,

    TradeDate DATE NOT NULL,

    OpenPrice DECIMAL(18,4) NULL,

    HighPrice DECIMAL(18,4) NULL,

    LowPrice DECIMAL(18,4) NULL,

    ClosePrice DECIMAL(18,4) NULL,

    ChangeAmount DECIMAL(18,4) NULL,

    ChangePercent DECIMAL(12,6) NULL,

    Volume BIGINT NULL,

    Amount DECIMAL(24,4) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Market_IndexDailyPrice_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Market_IndexDailyPrice_Index
        FOREIGN KEY (IndexId)
        REFERENCES Market.IndexInfo(IndexId),

    CONSTRAINT UQ_Market_IndexDailyPrice_IndexDate
        UNIQUE (IndexId, TradeDate)
);
GO
