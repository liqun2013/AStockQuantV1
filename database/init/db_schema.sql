/*
====================================================
 AStockQuant V2
 SQL Server 2019

 Database Create Script

 Purpose:
 Value Investment Quantitative Analysis System

 Model:
 Buffett + Graham + Fisher

====================================================
*/


USE master;
GO


IF EXISTS
(
    SELECT 1
    FROM sys.databases
    WHERE name = 'AStockQuant'
)
BEGIN

    ALTER DATABASE AStockQuant
    SET SINGLE_USER
    WITH ROLLBACK IMMEDIATE;


    DROP DATABASE AStockQuant;

END

GO



CREATE DATABASE AStockQuant

ON PRIMARY
(
    NAME = N'AStockQuant_Data',

    FILENAME =
    N'/var/opt/mssql/data/AStockQuant_Data.mdf',

    SIZE = 1024MB,

    FILEGROWTH = 512MB
)


LOG ON
(
    NAME = N'AStockQuant_Log',

    FILENAME =
    N'/var/opt/mssql/data/AStockQuant_Log.ldf',

    SIZE = 512MB,

    FILEGROWTH = 256MB
);


GO



ALTER DATABASE AStockQuant
SET RECOVERY FULL;

GO



ALTER DATABASE AStockQuant
SET AUTO_CLOSE OFF;

GO



ALTER DATABASE AStockQuant
SET AUTO_SHRINK OFF;

GO



ALTER DATABASE AStockQuant
SET READ_COMMITTED_SNAPSHOT ON;

GO



/*
====================================================
 AStockQuant V2

 Schema Create Script

 SQL Server 2019

 Model:
 Buffett + Graham + Fisher

====================================================
*/


USE AStockQuant;

GO


/*
====================================================
 1. Basic

 基础资料

 股票、行业、交易所
====================================================
*/

CREATE SCHEMA Basic;

GO



/*
====================================================
 2. Market

 行情数据

 日线、实时行情
====================================================
*/

CREATE SCHEMA Market;

GO



/*
====================================================
 3. Finance

 财务数据

 三大财务报表
====================================================
*/

CREATE SCHEMA Finance;

GO



/*
====================================================
 4. Buffett

 巴菲特企业质量模型

 ROE
 ROIC
 FCF
 护城河

====================================================
*/

CREATE SCHEMA Buffett;

GO



/*
====================================================
 5. Graham

 格雷厄姆安全边际模型

 PE
 PB
 Graham Number

====================================================
*/

CREATE SCHEMA Graham;

GO



/*
====================================================
 6. Fisher

 菲利普费雪成长模型

 成长性
 行业空间

====================================================
*/

CREATE SCHEMA Fisher;

GO



/*
====================================================
 7. Valuation

 估值体系

 PE/PB
 DCF
 内在价值

====================================================
*/

CREATE SCHEMA Valuation;

GO



/*
====================================================
 8. Industry

 行业比较

 市占率
 行业排名

====================================================
*/

CREATE SCHEMA Industry;

GO



/*
====================================================
 9. Risk

 风险控制

 波动
 回撤
 财务风险

====================================================
*/

CREATE SCHEMA Risk;

GO



/*
====================================================
 10. Quant

 综合量化评分

 股票综合排名

====================================================
*/

CREATE SCHEMA Quant;

GO



/*
====================================================
 11. Strategy

 策略管理

 股票池
 回测

====================================================
*/

CREATE SCHEMA Strategy;

GO



/*
====================================================
 12. Portfolio

 投资组合管理

 持仓
 交易记录

====================================================
*/

CREATE SCHEMA Portfolio;

GO



/*
====================================================
 13. System

 系统管理

 数据同步
 日志

====================================================
*/

CREATE SCHEMA System;

GO

/*

AStockQuant
│
├── Basic
│   ├── Exchange
│   ├── Industry
│   ├── Stock
│   └── StockIndustry
│
├── Market
│   ├── StockDailyPrice
│   ├── StockRealtimeQuote
│   ├── IndexInfo
│   └── IndexDailyPrice
│
├── Finance
│   ├── FinancialReport
│   ├── IncomeStatement
│   ├── BalanceSheet
│   ├── CashFlowStatement
│   ├── FinancialIndicator
│   ├── Dividend
│   ├── FinancialMetricDefinition
│   └── FinancialMetricValue
│
├── Buffett
│   ├── Indicator
│   ├── ScoreRule
│   ├── StockIndicatorValue
│   └── StockScore
│
├── Graham
│   ├── Indicator
│   ├── ScoreRule
│   ├── StockIndicatorValue
│   └── StockScore
│
├── Fisher
│   ├── Indicator
│   ├── ScoreRule
│   ├── StockIndicatorValue
│   └── StockScore
│
├── Valuation
│   ├── MarketValue
│   ├── PERatio
│   ├── PBRatio
│   ├── PSRatio
│   ├── DividendYield
│   ├── HistoricalPercentile
│   ├── GrahamValue
│   ├── DCFModel
│   ├── DCFProjection
│   ├── DCFResult
│   ├── IntrinsicValue
│   └── ValuationScore
│
├── Industry
│   ├── Indicator
│   └── StockIndustryScore
│
├── Risk
│   ├── StockRiskIndicator
│   └── CycleIndicator
│
├── Quant
│   ├── ScoreModel
│   ├── ScoreModelWeight
│   └── InvestmentScore
│
├── Strategy
│   ├── StockPool
│   ├── StockPoolItem
│   ├── BacktestStrategy
│   ├── BacktestRun
│   └── BacktestTrade
│
├── Portfolio
│   ├── Account
│   ├── Position
│   ├── TradeTransaction
│   ├── PortfolioSnapshot
│   ├── InvestmentReview
│   └── DividendRecord
│
└── System
    ├── DataSource
    ├── DataImportTask
    ├── DataImportLog
    └── SystemSetting

*/


/*

AStockQuant SQL Server 2019
│
├── 01_CreateDatabase.sql
│
├── 02_CreateSchema.sql
│
├── 03_CreateTables.sql
│   ├── Part 1 Basic + Market
│   ├── Part 2 Finance
│   ├── Part 3 Buffett + Graham + Fisher
│   ├── Part 4 Valuation
│   ├── Part 5 Industry + Risk + Quant + Strategy
│   └── Part 6 Portfolio + System
│
├── 04_CreateIndexes.sql
│
├── 05_InsertInvestmentRules.sql
│
├── 06_CreateViews.sql
│
└── 07_CreateStoredProcedures.sql

*/