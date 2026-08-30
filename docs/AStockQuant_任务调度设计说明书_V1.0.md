# AStockQuant 任务调度设计说明书

**版本：V1.0**  
**项目名称：AStockQuant 股票量化分析系统**  
**文档类型：Task Scheduling Design Document（任务调度设计说明书）**  
**状态：设计基线版本**  
**更新时间：2026-08-26**

---

# 目录

- [1. 文档说明](#1-文档说明)
- [2. 任务调度总体设计](#2-任务调度总体设计)
- [3. Worker Service架构设计](#3-worker-service架构设计)
- [4. 任务分类设计](#4-任务分类设计)
- [5. 行情数据同步任务设计](#5-行情数据同步任务设计)
- [6. 财务数据同步任务设计](#6-财务数据同步任务设计)
- [7. 指标计算任务设计](#7-指标计算任务设计)
- [8. 股票评分任务设计](#8-股票评分任务设计)
- [9. 股票排名生成任务设计](#9-股票排名生成任务设计)
- [10. 任务执行流程设计](#10-任务执行流程设计)
- [11. 任务状态管理设计](#11-任务状态管理设计)
- [12. 失败重试机制设计](#12-失败重试机制设计)
- [13. 并发控制设计](#13-并发控制设计)
- [14. 任务监控设计](#14-任务监控设计)
- [15. 扩展设计](#15-扩展设计)

---

# 1. 文档说明

## 1.1 文档目的

本文档定义 AStockQuant 后台任务调度体系。

目标：

- 自动完成数据同步；
- 自动完成指标计算；
- 自动生成股票分析结果；
- 保证任务稳定运行。

---

## 1.2 适用范围

覆盖：

- 数据同步；
- 数据清洗；
- 指标计算；
- 策略计算；
- 排名生成；
- 系统维护。

---

# 2. 任务调度总体设计


AStockQuant采用：

> Worker Service + 定时任务 + 任务状态管理


整体架构：


```text
                 Scheduler

                    |

             Worker Service

                    |

        -----------------------

        |          |          |

   Data Sync   Calculate   Ranking

        |

   SQL Server
```


---

# 3. Worker Service架构设计


## 3.1 项目定位


项目：

```text
AStockQuant.Worker
```


职责：

- 后台长期运行；
- 执行定时任务；
- 管理任务生命周期。


---

## 3.2 技术方案


基于：

- .NET 10 Worker Service；
- BackgroundService；
- Dependency Injection。


---

## 3.3 代码结构


```text
AStockQuant.Worker

├── Jobs

│   ├── DailyPriceSyncJob

│   ├── FinancialSyncJob

│   ├── IndicatorCalculateJob

│   └── RankingGenerateJob


├── Scheduler

├── Services

└── Configuration
```


---

# 4. 任务分类设计


系统任务分为：

|类型|说明|
|-|-|
|数据采集任务|获取外部数据|
|数据处理任务|清洗转换|
|计算任务|指标计算|
|分析任务|模型评分|
|维护任务|系统维护|

---

# 5. 行情数据同步任务设计


## 5.1 任务名称


```text
DailyPriceSyncJob
```


---

## 5.2 执行时间


交易日：

```text
16:00
```


---

## 5.3 执行流程


```text
Start

 |

获取股票列表

 |

调用AKTools行情接口

 |

数据转换

 |

数据验证

 |

保存数据库

 |

记录日志

 |

End
```


---

## 5.4 异常处理


失败：

- 自动重试；
- 记录失败原因；
- 等待人工处理。


---

# 6. 财务数据同步任务设计


## 6.1 任务名称


```text
FinancialSyncJob
```


---

## 6.2 执行周期


建议：

每日检查。

财报发布期间：

增加频率。


---

## 6.3 流程


```text
查询待更新股票

↓

调用财务接口

↓

解析财报

↓

保存数据库

↓

更新状态
```


---

# 7. 指标计算任务设计


## 7.1 任务名称


```text
IndicatorCalculateJob
```


---

## 7.2 功能


计算：

- ROE；
- ROIC；
- PE；
- PB；
- 成长率。


---

## 7.3 流程


```text
Financial Data

↓

Indicator Engine

↓

Indicator Result

↓

保存
```


---

# 8. 股票评分任务设计


## 8.1 任务名称


```text
StockScoreCalculateJob
```


---

## 8.2 输入数据


包括：

- 指标结果；
- 财务质量；
- 估值数据。


---

## 8.3 输出


生成：

```text
Stock_Score_Result
```


---

流程：

```text
Indicators

↓

Fisher Model

↓

Buffett Model

↓

Graham Model

↓

Total Score
```


---

# 9. 股票排名生成任务设计


## 9.1 任务名称


```text
RankingGenerateJob
```


---

## 9.2 功能


生成：

- 综合排名；
- 模型排名；
- 行业排名。


---

## 9.3 流程


```text
Score Result

↓

排序

↓

生成Ranking

↓

保存
```


---

# 10. 任务执行流程设计


标准流程：


```text
任务触发

↓

创建Task Instance

↓

执行任务

↓

更新状态

↓

记录日志

↓

完成
```


---

# 11. 任务状态管理设计


## 11.1 状态定义


|状态|说明|
|-|-|
|Pending|等待执行|
|Running|执行中|
|Success|成功|
|Failed|失败|
|Retrying|重试|

---

## 11.2 任务记录表


建议：

```text
Task_Execution_Log
```


字段：

|字段|说明|
|-|-|
|TaskName|任务名称|
|StartTime|开始时间|
|EndTime|结束时间|
|Status|状态|
|Message|信息|

---

# 12. 失败重试机制设计


## 12.1 重试策略


采用：

指数退避。


例如：

```text
第一次失败

等待1分钟


第二次失败

等待5分钟


第三次失败

等待30分钟
```


---

## 12.2 最大重试次数


默认：

```text
3次
```


超过：

进入失败状态。

---

# 13. 并发控制设计


## 13.1 防止重复执行


例如：

行情同步任务：

不能同时运行两个实例。


---

方案：

任务锁：

```text
Task_Lock
```


---

## 13.2 批处理控制


大量股票：

采用：

```text
Batch Processing
```


例如：

每批：

500只股票。


---

# 14. 任务监控设计


监控内容：

## 任务状态

查看：

- 当前运行任务；
- 最近执行时间；
- 成功率。


---

## 数据状态


检查：

- 最新行情日期；
- 财务数据日期；
- 指标更新时间。


---

## 告警机制


异常：

通知：

- 管理后台；
- 邮件；
- 企业微信（未来）。

---

# 15. 扩展设计


## 15.1 分布式任务调度


未来支持：

- Quartz.NET；
- Hangfire；
- 分布式Scheduler。


---

## 15.2 多Worker扩展


架构：

```text
          Scheduler

              |

      -----------------

      Worker1

      Worker2

      Worker3
```


---

## 15.3 AI分析任务


未来增加：

```text
Financial Report

↓

AI Analysis Job

↓

Risk Report
```


---

# 文档结束


文件名称：

```text
AStockQuant_任务调度设计说明书_V1.0.md
```

状态：

Task Scheduling Design Baseline V1.0