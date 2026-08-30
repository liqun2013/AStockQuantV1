1. 系统总体架构设计
1.1 架构目标

AStockQuant 采用：

Clean Architecture + Domain Driven Design（DDD）

设计原则：

领域逻辑独立
数据访问可替换
投资模型可扩展
支持后台任务
支持未来 AI 分析
1.2 系统总体架构
                     用户
                      |
                      |
                Web / Mobile UI
                      |
                      |
              ASP.NET Core Web API
                      |
        --------------------------------
        |                              |
 Application Layer              Background Worker
        |                              |
        |
 Domain Layer
        |
        |
 Infrastructure Layer
        |
 --------------------------------------
 |                |                   |
SQL Server     AKTools API       File Storage

2. .NET Solution设计

最终项目结构：

AStockQuant.sln

src

├── AStockQuant.Domain
│
├── AStockQuant.Application
│
├── AStockQuant.Infrastructure
│
├── AStockQuant.Data
│
├── AStockQuant.WebApi
│
├── AStockQuant.Worker
│
└── AStockQuant.Shared


tests

├── AStockQuant.Domain.Tests
│
├── AStockQuant.Application.Tests
│
└── AStockQuant.Integration.Tests
3. 各层职责设计
3.1 Domain Layer

项目：

AStockQuant.Domain

职责：

存放：

实体
值对象
领域规则
投资模型

禁止：

❌ SQL

❌ Dapper

❌ HttpClient

❌ EF Core

Domain结构
Domain

├── Entities

├── ValueObjects

├── Enums

├── Interfaces

├── Models

└── Services

3.2 Application Layer

项目：

AStockQuant.Application

职责：

协调业务流程。

例如：

股票分析：

Controller

 ↓

StockAnalysisService

 ↓

Repository

 ↓

Database


包含：

Application Service
DTO
Command
Query
Mapper

结构：

Application

├── Services

├── DTOs

├── Interfaces

├── Commands

├── Queries

└── Validators

3.3 Infrastructure Layer

项目：

AStockQuant.Infrastructure

职责：

外部实现。

包括：

数据库
Dapper Repository
AKTools
AKTools Client
文件
Export Service

结构：

Infrastructure

├── Persistence

│   ├── ConnectionFactory
│   ├── Repository

├── AKTools

│   ├── Client
│   ├── Models

├── Logging

└── Services

3.4 WebApi Layer

项目：

AStockQuant.WebApi

职责：

HTTP接口。

结构：

WebApi

├── Controllers

├── Middleware

├── Filters

├── Authentication

└── Extensions

3.5 Worker Layer

后台任务。

项目：

AStockQuant.Worker

负责：

数据同步：

每天16:00

↓

同步行情

↓

计算指标

↓

生成评分

4. 核心领域模型设计
4.1 Stock股票实体

对应：

Basic.Stock
public class Stock
{
    public int StockId {get;set;}

    public string StockCode {get;set;}

    public string StockName {get;set;}

    public int ExchangeId {get;set;}

    public DateTime? ListingDate {get;set;}

    public bool IsActive {get;set;}
}
4.2 FinancialReport财务报告

对应：

Finance.FinancialReport
public class FinancialReport
{
    public long ReportId {get;set;}

    public int StockId {get;set;}

    public DateTime ReportPeriod {get;set;}

    public DateTime PublishDate {get;set;}

    public string ReportType {get;set;}
}
4.3 LongTermMetric长期指标

对应：

Finance.LongTermMetric
public class LongTermMetric
{
    public long Id {get;set;}

    public int StockId {get;set;}

    public string MetricCode {get;set;}

    public decimal MetricValue {get;set;}

    public DateTime DataAsOfDate {get;set;}
}
4.4 InvestmentScore综合评分

对应：

Quant.InvestmentScore
public class InvestmentScore
{
    public long ScoreId {get;set;}

    public int StockId {get;set;}

    public decimal BuffettScore {get;set;}

    public decimal GrahamScore {get;set;}

    public decimal FisherScore {get;set;}

    public decimal FinalScore {get;set;}

    public string Rating {get;set;}
}
5. Repository设计

原则：

Domain只定义接口。

Infrastructure实现。

Stock Repository

Domain:

public interface IStockRepository
{
    Task<Stock?> GetAsync(
        int stockId);

    Task<IReadOnlyList<Stock>>
        SearchAsync(
        string keyword);
}

Infrastructure:

public class StockRepository
    : IStockRepository
{

}
6. Application Service设计
股票分析服务

接口：

public interface IStockAnalysisService
{

    Task<StockAnalysisDto>
        GetAnalysisAsync(
        int stockId,
        DateTime? asOfDate);

}

返回：

{
 "code":"600519",

 "name":"贵州茅台",

 "buffettScore":92,

 "grahamScore":78,

 "fisherScore":90,

 "finalScore":87,

 "rating":"A"
}
7. 投资评分引擎设计

这是系统核心。

结构：

ScoreEngine

        |
        |
 -----------------------
 |          |           |
Buffett   Graham     Fisher
Score     Score      Score

        |
        |

InvestmentScore

Buffett评分接口
public interface IBuffettScoreCalculator
{

    ScoreResult Calculate(
        FinancialSnapshot data);

}
Graham评分接口
public interface IGrahamScoreCalculator
{

    ScoreResult Calculate(
        ValuationSnapshot data);

}
Fisher评分接口
public interface IFisherScoreCalculator
{

    ScoreResult Calculate(
        GrowthSnapshot data);

}
8. 数据导入设计

AKTools：

AKTools API

      ↓

AKToolsClient

      ↓

ImportService

      ↓

Repository

      ↓

SQL Server

Import流程

例如行情：

Worker启动

↓

创建ImportTask

↓

调用AKTools

↓

获取股票行情

↓

数据验证

↓

批量写入

↓

记录ImportLog

9. 后台任务设计

使用：

.NET BackgroundService

任务：

任务	周期
行情同步	每日
财务同步	季度
估值计算	每日
评分计算	每日
风险计算	每日
10. API设计原则

RESTful。

股票分析
GET

/api/stocks/{code}/analysis


返回：

{
"stockCode":"600519",

"qualityScore":90,

"valueScore":80,

"growthScore":88,

"finalScore":86
}
股票排行
GET

/api/ranking

参数：

model=VALUE_INVESTMENT

top=100
回测
POST

/api/backtest

输入：

{
"strategyId":1,

"start":"2018-01-01",

"end":"2026-01-01"
}
11. 日志设计

统一：

Serilog

记录：

API调用
数据导入
异常
评分计算
12. 异常处理

统一返回：

{
"success":false,

"code":"STOCK_NOT_FOUND",

"message":"股票不存在"
}
13. 测试设计
Unit Test

测试：

Buffett计算
Graham计算
Fisher计算
Integration Test

测试：

SQL连接
Repository
API
14. 开发顺序建议

重新开始代码时：

Step 1

创建 Solution

AStockQuant.sln
Step 2

创建：

Domain
Application
Infrastructure
WebApi
Worker
Tests
Step 3

先实现：

Database Connection

Repository

AKTools Client
Step 4

实现：

Import Pipeline
Step 5

实现：

Financial Engine

Score Engine
Step 6

实现：

Web API
15. 设计冻结原则

以后开发必须遵守：

数据层

数据库已经冻结。

禁止：

随意新增字段。

Domain层

投资逻辑只能在 Domain。

禁止：

Controller计算评分。

Application层

负责流程。

禁止：

写SQL。

Infrastructure层

负责：

数据库/API/文件。

结论

AStockQuant V2.0 技术设计已经确定：

需求
 ↓
系统设计
 ↓
数据库
 ↓
Domain
 ↓
Application
 ↓
Infrastructure
 ↓
API
 ↓
Worker