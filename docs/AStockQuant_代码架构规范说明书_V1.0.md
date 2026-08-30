# AStockQuant 代码架构规范说明书

**版本：V1.0**  
**项目名称：AStockQuant 股票量化分析系统**  
**文档类型：Code Architecture Specification（代码架构规范说明书）**  
**状态：开发基线版本**  
**更新时间：2026-08-26**

---

# 目录

- [1. 文档说明](#1-文档说明)
- [2. 代码架构设计原则](#2-代码架构设计原则)
- [3. Solution整体结构规范](#3-solution整体结构规范)
- [4. 项目职责规范](#4-项目职责规范)
- [5. DDD分层规范](#5-ddd分层规范)
- [6. Namespace规范](#6-namespace规范)
- [7. Entity设计规范](#7-entity设计规范)
- [8. DTO设计规范](#8-dto设计规范)
- [9. Repository设计规范](#9-repository设计规范)
- [10. Service设计规范](#10-service设计规范)
- [11. Dependency Injection规范](#11-dependency-injection规范)
- [12. 数据访问规范](#12-数据访问规范)
- [13. 异常处理规范](#13-异常处理规范)
- [14. 日志规范](#14-日志规范)
- [15. 配置管理规范](#15-配置管理规范)
- [16. 单元测试规范](#16-单元测试规范)
- [17. Git开发规范](#17-git开发规范)
- [18. 后续扩展规范](#18-后续扩展规范)

---

# 1. 文档说明

## 1.1 文档目的

本文档定义 AStockQuant 项目的代码组织规则和开发规范。

目标：

- 保持代码结构清晰；
- 降低长期维护成本；
- 支持多人协作开发；
- 保证系统可持续扩展。

---

## 1.2 适用范围

适用于：

- Web项目；
- API项目；
- Worker任务；
- 数据服务；
- 核心业务模块。

---

# 2. 代码架构设计原则

AStockQuant采用：

> Clean Architecture + DDD思想 + 模块化设计

核心原则：

---

## 2.1 依赖方向单向


```text
Web

 ↓

Application

 ↓

Domain


Infrastructure

 ↓

Database / External API
```

规则：

- Domain 不依赖任何外部组件；
- Application 不直接访问数据库；
- Infrastructure 实现外部能力。

---

## 2.2 高内聚低耦合

禁止：

- Controller写业务逻辑；
- Entity调用数据库；
- Repository包含复杂业务。


---

## 2.3 面向扩展设计

新增能力：

例如：

增加投资模型：

```text
InvestmentModels

    |

New Model

    |

No Impact Existing Code
```

---

# 3. Solution整体结构规范


标准结构：


```text
AStockQuant

├── src

│
├── AStockQuant.Domain

│
├── AStockQuant.Application

│
├── AStockQuant.Infrastructure

│
├── AStockQuant.DataProvider

│
├── AStockQuant.Web

│
├── AStockQuant.Worker


├── tests

│
├── AStockQuant.UnitTests

│
└── AStockQuant.IntegrationTests


└── docs
```

---

# 4. 项目职责规范


# 4.1 Domain项目


职责：

保存核心业务。


包含：

```text
Entities

ValueObjects

Enums

Domain Services

Business Rules
```

---

禁止：

```text
SQL

HTTP

File IO

Configuration
```

---

# 4.2 Application项目


职责：

业务流程编排。


包含：

```text
Application Services

DTO

Interfaces

Commands

Queries
```

---

示例：

```text
CalculateStockScoreService
```

---

# 4.3 Infrastructure项目


职责：

实现技术细节。


包含：

```text
Database

Repository

Cache

Logging

External Service
```

---

# 4.4 DataProvider项目


职责：

第三方数据接口。


包含：

```text
AKToolsClient

MarketDataProvider

FinancialDataProvider
```

---

# 4.5 Web项目


职责：

用户访问入口。


包含：

```text
Controllers

Views

API

Authentication
```

---

# 4.6 Worker项目


职责：

后台任务。


例如：

```text
DailyStockSyncJob

IndicatorCalculateJob

RankingGenerateJob
```

---

# 5. DDD分层规范


推荐结构：


```text
Domain

 ├── Stock

 │    ├── Stock.cs

 │    ├── StockPrice.cs


Application

 ├── Stock

 │    ├── StockService.cs


Infrastructure

 ├── Repository

 │    ├── StockRepository.cs
```

---

# 6. Namespace规范


统一：

```csharp
AStockQuant.{Project}.{Module}
```


示例：

```csharp
AStockQuant.Domain.Stock;

AStockQuant.Application.Stock;

AStockQuant.Infrastructure.Repository;
```

---

# 7. Entity设计规范


Entity要求：

- 有唯一ID；
- 包含业务行为；
- 不暴露数据库细节。


示例：


```csharp
public class Stock
{
    public string Code { get; private set; }

    public string Name { get; private set; }
}
```

---

禁止：

```csharp
public class Stock
{
    public string CreateSql()
}
```

---

# 8. DTO设计规范


DTO用于：

- API传输；
- 服务通信；
- 数据转换。


示例：


```csharp
public class StockDto
{
    public string Code {get;set;}

    public string Name {get;set;}
}
```

---

规则：

Entity ≠ DTO

禁止直接返回Entity。

---

# 9. Repository设计规范


接口位置：

```text
Application
```

实现位置：

```text
Infrastructure
```


示例：


接口：

```csharp
public interface IStockRepository
{
    Task<Stock?> GetAsync(string code);
}
```


实现：

```csharp
public class StockRepository
    : IStockRepository
{
}
```

---

# 10. Service设计规范


Service负责：

- 业务流程；
- 模型调用；
- 数据组合。


例如：


```text
StockAnalysisService

    |

FisherEngine

    |

BuffettEngine

    |

GrahamEngine
```

---

禁止：

Controller直接调用Repository。

---

# 11. Dependency Injection规范


所有服务：

采用DI注册。


示例：


```csharp
services.AddScoped<IStockService,StockService>();
```


生命周期：

|类型|生命周期|
|-|-|
|Repository|Scoped|
|Service|Scoped|
|Client|Transient|
|Cache|Singleton|

---

# 12. 数据访问规范


统一：

Repository访问数据库。


禁止：

Controller：

```csharp
db.Stock.Where()
```


正确：

```text
Controller

↓

Service

↓

Repository

↓

Database
```

---

# 13. 异常处理规范


统一异常：


```csharp
BusinessException
```


分类：

|异常|说明|
|-|-|
|ValidationException|参数错误|
|BusinessException|业务错误|
|DataException|数据错误|
|SystemException|系统错误|

---

# 14. 日志规范


采用：

ILogger


日志等级：

|级别|用途|
|-|-|
|Trace|详细调试|
|Information|正常流程|
|Warning|异常情况|
|Error|错误|
|Critical|严重故障|

---

禁止：

```csharp
Console.WriteLine()
```

---

# 15. 配置管理规范


统一：

```text
appsettings.json
```


配置：

包括：

- 数据库连接；
- AKTools地址；
- 日志配置；
- Token配置。


---

禁止：

代码硬编码：

```csharp
"http://xxx"
```

---

# 16. 单元测试规范


测试项目：

```text
tests

├── UnitTests

└── IntegrationTests
```

---

测试重点：

- 指标计算；
- 评分模型；
- 数据转换；
- Repository。


---

# 17. Git开发规范


## 17.1 分支策略


```text
main

 |

develop

 |

feature/*
```


---

## 17.2 Commit规范


格式：

```text
Type: Description
```


例如：

```text
feat: add stock indicator engine

fix: fix financial parser

docs: update design document
```

---

# 18. 后续扩展规范


## 新增投资模型


步骤：

1. 创建Model类；

2. 实现统一接口；

3. 注册DI；

4. 增加测试。


---

接口：

```csharp
IInvestmentModel
```


实现：

```text
FisherModel

BuffettModel

GrahamModel
```

---

## 新增数据源


实现：

```csharp
IStockDataProvider
```


例如：

```text
AKToolsProvider

WindProvider

TushareProvider
```

---

# 文档结束


文件名称：

```text
AStockQuant_代码架构规范说明书_V1.0.md
```

状态：

Code Architecture Baseline V1.0