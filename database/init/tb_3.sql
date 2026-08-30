/*
====================================================
 Buffett.Indicator
 巴菲特指标定义

 Basic.Stock
      │
      ├──────── Buffett.StockIndicatorValue
      │                    ↓
      │              Buffett.StockScore
      │
      ├──────── Graham.StockIndicatorValue
      │                    ↓
      │              Graham.StockScore
      │
      └──────── Fisher.StockIndicatorValue
                           ↓
                     Fisher.StockScore
====================================================
*/

USE AStockQuant;
GO

CREATE TABLE Buffett.Indicator
(
    IndicatorId INT IDENTITY(1,1)
        CONSTRAINT PK_Buffett_Indicator PRIMARY KEY,

    IndicatorCode VARCHAR(20) NOT NULL,

    IndicatorName NVARCHAR(100) NOT NULL,

    Category VARCHAR(50) NOT NULL,

    Weight DECIMAL(10,4) NOT NULL,

    MaxScore DECIMAL(10,4) NOT NULL
        CONSTRAINT DF_Buffett_Indicator_MaxScore
        DEFAULT (10),

    Description NVARCHAR(1000) NULL,

    IsQuantitative BIT NOT NULL
        CONSTRAINT DF_Buffett_Indicator_IsQuantitative
        DEFAULT (1),

    IsActive BIT NOT NULL
        CONSTRAINT DF_Buffett_Indicator_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Buffett_Indicator_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Buffett_Indicator_Code
        UNIQUE (IndicatorCode),

    CONSTRAINT CK_Buffett_Indicator_Weight
        CHECK (Weight >= 0),

    CONSTRAINT CK_Buffett_Indicator_MaxScore
        CHECK (MaxScore > 0)
);
GO
/*
====================================================
 Buffett.ScoreRule
 巴菲特评分规则
====================================================
*/

CREATE TABLE Buffett.ScoreRule
(
    RuleId INT IDENTITY(1,1)
        CONSTRAINT PK_Buffett_ScoreRule PRIMARY KEY,

    IndicatorId INT NOT NULL,

    MinValue DECIMAL(24,8) NULL,

    MaxValue DECIMAL(24,8) NULL,

    Score DECIMAL(10,4) NOT NULL,

    RuleOrder INT NOT NULL,

    Description NVARCHAR(500) NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Buffett_ScoreRule_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Buffett_ScoreRule_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Buffett_ScoreRule_Indicator
        FOREIGN KEY (IndicatorId)
        REFERENCES Buffett.Indicator(IndicatorId),

    CONSTRAINT CK_Buffett_ScoreRule_ValueRange
        CHECK
        (
            MaxValue IS NULL
            OR MinValue IS NULL
            OR MaxValue >= MinValue
        ),

    CONSTRAINT CK_Buffett_ScoreRule_Score
        CHECK (Score >= 0)
);
GO
/*
====================================================
 Buffett.StockIndicatorValue
 股票 Buffett 指标计算结果
====================================================
*/

CREATE TABLE Buffett.StockIndicatorValue
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Buffett_StockIndicatorValue PRIMARY KEY,

    StockId INT NOT NULL,

    IndicatorId INT NOT NULL,

    CalcDate DATE NOT NULL,

    SourceReportId BIGINT NULL,

    IndicatorValue DECIMAL(24,8) NULL,

    RawValue DECIMAL(24,8) NULL,

    Score DECIMAL(10,4) NULL,

    DataAsOfDate DATE NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Buffett_StockIndicatorValue_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Buffett_StockIndicatorValue_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT FK_Buffett_StockIndicatorValue_Indicator
        FOREIGN KEY (IndicatorId)
        REFERENCES Buffett.Indicator(IndicatorId),

    CONSTRAINT FK_Buffett_StockIndicatorValue_Report
        FOREIGN KEY (SourceReportId)
        REFERENCES Finance.FinancialReport(ReportId),

    CONSTRAINT UQ_Buffett_StockIndicatorValue
        UNIQUE
        (
            StockId,
            IndicatorId,
            CalcDate
        )
);
GO
/*
====================================================
 Buffett.StockScore
 巴菲特综合评分
====================================================
*/

CREATE TABLE Buffett.StockScore
(
    ScoreId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Buffett_StockScore PRIMARY KEY,

    StockId INT NOT NULL,

    ScoreDate DATE NOT NULL,

    QualityScore DECIMAL(10,4) NULL,

    CashFlowScore DECIMAL(10,4) NULL,

    MoatScore DECIMAL(10,4) NULL,

    CapitalAllocationScore DECIMAL(10,4) NULL,

    TotalScore DECIMAL(10,4) NOT NULL,

    Grade VARCHAR(10) NULL,

    ModelVersion VARCHAR(30) NOT NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Buffett_StockScore_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Buffett_StockScore_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT UQ_Buffett_StockScore
        UNIQUE
        (
            StockId,
            ScoreDate,
            ModelVersion
        )
);
GO
/*
====================================================
 Graham.Indicator
 格雷厄姆指标定义
====================================================
*/

CREATE TABLE Graham.Indicator
(
    IndicatorId INT IDENTITY(1,1)
        CONSTRAINT PK_Graham_Indicator PRIMARY KEY,

    IndicatorCode VARCHAR(20) NOT NULL,

    IndicatorName NVARCHAR(100) NOT NULL,

    Category VARCHAR(50) NOT NULL,

    Weight DECIMAL(10,4) NOT NULL,

    MaxScore DECIMAL(10,4) NOT NULL
        CONSTRAINT DF_Graham_Indicator_MaxScore
        DEFAULT (10),

    Description NVARCHAR(1000) NULL,

    IsQuantitative BIT NOT NULL
        CONSTRAINT DF_Graham_Indicator_IsQuantitative
        DEFAULT (1),

    IsActive BIT NOT NULL
        CONSTRAINT DF_Graham_Indicator_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Graham_Indicator_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Graham_Indicator_Code
        UNIQUE (IndicatorCode)
);
GO
/*
====================================================
 Graham.ScoreRule
 格雷厄姆评分规则
====================================================
*/

CREATE TABLE Graham.ScoreRule
(
    RuleId INT IDENTITY(1,1)
        CONSTRAINT PK_Graham_ScoreRule PRIMARY KEY,

    IndicatorId INT NOT NULL,

    MinValue DECIMAL(24,8) NULL,

    MaxValue DECIMAL(24,8) NULL,

    Score DECIMAL(10,4) NOT NULL,

    RuleOrder INT NOT NULL,

    Description NVARCHAR(500) NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Graham_ScoreRule_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Graham_ScoreRule_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Graham_ScoreRule_Indicator
        FOREIGN KEY (IndicatorId)
        REFERENCES Graham.Indicator(IndicatorId),

    CONSTRAINT CK_Graham_ScoreRule_ValueRange
        CHECK
        (
            MaxValue IS NULL
            OR MinValue IS NULL
            OR MaxValue >= MinValue
        ),

    CONSTRAINT CK_Graham_ScoreRule_Score
        CHECK (Score >= 0)
);
GO
/*
====================================================
 Graham.StockIndicatorValue
 格雷厄姆指标结果
====================================================
*/

CREATE TABLE Graham.StockIndicatorValue
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Graham_StockIndicatorValue PRIMARY KEY,

    StockId INT NOT NULL,

    IndicatorId INT NOT NULL,

    CalcDate DATE NOT NULL,

    SourceReportId BIGINT NULL,

    IndicatorValue DECIMAL(24,8) NULL,

    RawValue DECIMAL(24,8) NULL,

    Score DECIMAL(10,4) NULL,

    DataAsOfDate DATE NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Graham_StockIndicatorValue_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Graham_StockIndicatorValue_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT FK_Graham_StockIndicatorValue_Indicator
        FOREIGN KEY (IndicatorId)
        REFERENCES Graham.Indicator(IndicatorId),

    CONSTRAINT FK_Graham_StockIndicatorValue_Report
        FOREIGN KEY (SourceReportId)
        REFERENCES Finance.FinancialReport(ReportId),

    CONSTRAINT UQ_Graham_StockIndicatorValue
        UNIQUE
        (
            StockId,
            IndicatorId,
            CalcDate
        )
);
GO
/*
====================================================
 Graham.StockScore
 格雷厄姆综合评分
====================================================
*/

CREATE TABLE Graham.StockScore
(
    ScoreId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Graham_StockScore PRIMARY KEY,

    StockId INT NOT NULL,

    ScoreDate DATE NOT NULL,

    ValuationScore DECIMAL(10,4) NULL,

    SafetyScore DECIMAL(10,4) NULL,

    EarningsStabilityScore DECIMAL(10,4) NULL,

    DividendScore DECIMAL(10,4) NULL,

    TotalScore DECIMAL(10,4) NOT NULL,

    Grade VARCHAR(10) NULL,

    ModelVersion VARCHAR(30) NOT NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Graham_StockScore_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Graham_StockScore_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT UQ_Graham_StockScore
        UNIQUE
        (
            StockId,
            ScoreDate,
            ModelVersion
        )
);
GO
/*
====================================================
 Fisher.Indicator
 菲利普费雪指标定义
====================================================
*/

CREATE TABLE Fisher.Indicator
(
    IndicatorId INT IDENTITY(1,1)
        CONSTRAINT PK_Fisher_Indicator PRIMARY KEY,

    IndicatorCode VARCHAR(20) NOT NULL,

    IndicatorName NVARCHAR(100) NOT NULL,

    Category VARCHAR(50) NOT NULL,

    Weight DECIMAL(10,4) NOT NULL,

    MaxScore DECIMAL(10,4) NOT NULL
        CONSTRAINT DF_Fisher_Indicator_MaxScore
        DEFAULT (10),

    Description NVARCHAR(1000) NULL,

    IsQuantitative BIT NOT NULL
        CONSTRAINT DF_Fisher_Indicator_IsQuantitative
        DEFAULT (1),

    IsActive BIT NOT NULL
        CONSTRAINT DF_Fisher_Indicator_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Fisher_Indicator_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Fisher_Indicator_Code
        UNIQUE (IndicatorCode)
);
GO
/*
====================================================
 Fisher.ScoreRule
 菲利普费雪评分规则
====================================================
*/

CREATE TABLE Fisher.ScoreRule
(
    RuleId INT IDENTITY(1,1)
        CONSTRAINT PK_Fisher_ScoreRule PRIMARY KEY,

    IndicatorId INT NOT NULL,

    MinValue DECIMAL(24,8) NULL,

    MaxValue DECIMAL(24,8) NULL,

    Score DECIMAL(10,4) NOT NULL,

    RuleOrder INT NOT NULL,

    Description NVARCHAR(500) NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Fisher_ScoreRule_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Fisher_ScoreRule_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Fisher_ScoreRule_Indicator
        FOREIGN KEY (IndicatorId)
        REFERENCES Fisher.Indicator(IndicatorId),

    CONSTRAINT CK_Fisher_ScoreRule_ValueRange
        CHECK
        (
            MaxValue IS NULL
            OR MinValue IS NULL
            OR MaxValue >= MinValue
        ),

    CONSTRAINT CK_Fisher_ScoreRule_Score
        CHECK (Score >= 0)
);
GO
/*
====================================================
 Fisher.StockIndicatorValue
 菲利普费雪指标结果
====================================================
*/

CREATE TABLE Fisher.StockIndicatorValue
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Fisher_StockIndicatorValue PRIMARY KEY,

    StockId INT NOT NULL,

    IndicatorId INT NOT NULL,

    CalcDate DATE NOT NULL,

    SourceReportId BIGINT NULL,

    IndicatorValue DECIMAL(24,8) NULL,

    RawValue DECIMAL(24,8) NULL,

    Score DECIMAL(10,4) NULL,

    DataAsOfDate DATE NULL,

    AnalystComment NVARCHAR(1000) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Fisher_StockIndicatorValue_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Fisher_StockIndicatorValue_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT FK_Fisher_StockIndicatorValue_Indicator
        FOREIGN KEY (IndicatorId)
        REFERENCES Fisher.Indicator(IndicatorId),

    CONSTRAINT FK_Fisher_StockIndicatorValue_Report
        FOREIGN KEY (SourceReportId)
        REFERENCES Finance.FinancialReport(ReportId),

    CONSTRAINT UQ_Fisher_StockIndicatorValue
        UNIQUE
        (
            StockId,
            IndicatorId,
            CalcDate
        )
);
GO
/*
====================================================
 Fisher.StockScore
 菲利普费雪综合评分
====================================================
*/

CREATE TABLE Fisher.StockScore
(
    ScoreId BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Fisher_StockScore PRIMARY KEY,

    StockId INT NOT NULL,

    ScoreDate DATE NOT NULL,

    StabilityScore DECIMAL(10,4) NULL,

    ProfitabilityScore DECIMAL(10,4) NULL,

    CashFlowScore DECIMAL(10,4) NULL,

    GrowthScore DECIMAL(10,4) NULL,

    CompetitiveAdvantageScore DECIMAL(10,4) NULL,

    TotalScore DECIMAL(10,4) NOT NULL,

    Grade VARCHAR(10) NULL,

    ModelVersion VARCHAR(30) NOT NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Fisher_StockScore_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Fisher_StockScore_Stock
        FOREIGN KEY (StockId)
        REFERENCES Basic.Stock(StockId),

    CONSTRAINT UQ_Fisher_StockScore
        UNIQUE
        (
            StockId,
            ScoreDate,
            ModelVersion
        )
);
GO
