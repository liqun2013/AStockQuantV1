# AStockQuant 数据库设计说明书（DD）

**版本：V1.0**  
**项目名称：AStockQuant 股票量化分析系统**  
**文档类型：Database Design Document（数据库设计说明书）**  
**状态：设计基线版本**  
**更新时间：2026-08-26**

---

# 目录

- [1. 文档说明](#1-文档说明)
- [2. 数据库总体设计](#2-数据库总体设计)
- [3. 数据库技术方案](#3-数据库技术方案)
- [4. 数据库分层设计](#4-数据库分层设计)
- [5. 核心业务表设计](#5-核心业务表设计)
- [6. 行情数据表设计](#6-行情数据表设计)
- [7. 财务数据表设计](#7-财务数据表设计)
- [8. 指标计算表设计](#8-指标计算表设计)
- [9. 投资模型表设计](#9-投资模型表设计)
- [10. 评分系统表设计](#10-评分系统表设计)
- [11. 数据同步表设计](#11-数据同步表设计)
- [12. 索引设计](#12-索引设计)
- [13. 数据维护设计](#13-数据维护设计)
- [14. 数据扩展设计](#14-数据扩展设计)

---

# 1. 文档说明

## 1.1 文档目的

本文档用于定义 AStockQuant 系统数据库结构设计。

目标：

- 明确数据库对象；
- 明确表职责；
- 明确数据关系；
- 指导数据库开发；
- 作为后续 SQL Script 与程序开发依据。

---

# 2. 数据库总体设计

## 2.1 数据库定位

AStockQuant 数据库用于保存：

- A 股基础信息；
- 股票行情数据；
- 上市公司财务数据；
- 投资指标数据；
- 策略计算结果；
- 系统运行日志。

---

## 2.2 数据流向

```text
外部数据源

    |

AKTools

    |

数据采集模块

    |

SQL Server

    |

指标计算

    |

投资分析结果
```

---

# 3. 数据库技术方案

## 3.1 数据库选型

|项目|方案|
|-|-|
|数据库|SQL Server 2019|
|字符集|UTF-8|
|访问方式|Dapper / FreeSql|
|部署方式|Docker / Windows Server|

---

## 3.2 设计原则

### 数据完整性

采用：

- 主键；
- 唯一约束；
- 外键约束；
- Check Constraint。

---

### 查询性能

采用：

- 聚集索引；
- 非聚集索引；
- 时间字段索引；
- 股票代码索引。

---

### 历史数据保存

行情数据长期保存：

- 不覆盖历史；
- 增量追加；
- 支持回测分析。

---

# 4. 数据库分层设计

数据库逻辑划分：

```text
AStockQuant

├── 基础数据

├── 行情数据

├── 财务数据

├── 指标数据

├── 策略数据

└── 系统数据
```

---

# 5. 核心业务表设计

# 5.1 股票基础信息表

表：

```text
Stock_Basic
```

用途：

保存股票基本信息。


主要字段：

|字段|说明|
|-|-|
|StockCode|股票代码|
|StockName|股票名称|
|Market|市场|
|Industry|行业|
|ListDate|上市日期|
|Status|状态|

---

# 5.2 行业分类表

表：

```text
Stock_Industry
```

用途：

保存行业信息。


字段：

|字段|说明|
|-|-|
|IndustryCode|行业代码|
|IndustryName|行业名称|

---

# 6. 行情数据表设计

# 6.1 日线行情表

表：

```text
Stock_Daily_Price
```

用途：

保存每日行情。


字段：

|字段|说明|
|-|-|
|StockCode|股票代码|
|TradeDate|交易日期|
|Open|开盘|
|High|最高|
|Low|最低|
|Close|收盘|
|Volume|成交量|
|Amount|成交额|

---

## 索引设计

主键：

```text
StockCode + TradeDate
```

索引：

```text
TradeDate
```

用于：

- 趋势查询；
- 历史分析。

---

# 7. 财务数据表设计

# 7.1 财务报表主表


表：

```text
Financial_Report
```

保存：

- 年报；
- 季报。


字段：

|字段|说明|
|-|-|
|StockCode|股票代码|
|ReportDate|报告日期|
|ReportType|报告类型|
|CreateTime|创建时间|

---

# 7.2 利润表

表：

```text
Financial_Income
```

字段：

包括：

- 营业收入；
- 营业利润；
- 净利润；
- 毛利率。


---

# 7.3 资产负债表

表：

```text
Financial_Balance
```

字段：

包括：

- 总资产；
- 总负债；
- 股东权益。


---

# 7.4 现金流量表

表：

```text
Financial_CashFlow
```

字段：

包括：

- 经营现金流；
- 自由现金流。


---

# 8. 指标计算表设计

# 8.1 股票指标结果表


表：

```text
Stock_Indicator_Result
```


用途：

保存计算后的投资指标。


字段：

|字段|说明|
|-|-|
|StockCode|股票代码|
|IndicatorCode|指标代码|
|IndicatorValue|指标值|
|CalculateDate|计算日期|

---

支持指标：

- ROE；
- ROIC；
- PE；
- PB；
- Revenue Growth；
- Profit Growth。

---

# 9. 投资模型表设计

# 9.1 投资模型定义表


表：

```text
Investment_Model
```


保存：

- Fisher；
- Buffett；
- Graham。


字段：

|字段|说明|
|-|-|
|ModelCode|模型编号|
|ModelName|模型名称|
|Description|说明|

---

# 9.2 模型规则表


表：

```text
Investment_Rule
```


字段：

|字段|说明|
|-|-|
|ModelCode|模型|
|IndicatorCode|指标|
|Weight|权重|
|RuleExpression|规则|

---

# 10. 评分系统表设计

# 10.1 股票评分结果表


表：

```text
Stock_Score_Result
```


字段：

|字段|说明|
|-|-|
|StockCode|股票代码|
|FisherScore|成长评分|
|BuffettScore|价值评分|
|GrahamScore|安全边际评分|
|TotalScore|综合评分|

---

# 10.2 股票排名表


表：

```text
Stock_Ranking
```


用途：

保存每日排名结果。


字段：

- 股票代码；
- 排名；
- 总评分；
- 生成日期。

---

# 11. 数据同步表设计

# 11.1 数据同步任务表


表：

```text
Data_Sync_Task
```


保存：

- 同步任务；
- 执行状态；
- 执行时间。


---

# 11.2 同步日志表


表：

```text
Data_Sync_Log
```


记录：

- 成功；
- 失败；
- 异常信息。

---

# 12. 索引设计

## 12.1 股票查询索引

重点字段：

```text
StockCode
```

---

## 12.2 时间查询索引

重点字段：

```text
TradeDate
ReportDate
CalculateDate
```

---

## 12.3 组合索引


行情：

```text
StockCode + TradeDate
```

财务：

```text
StockCode + ReportDate
```

指标：

```text
StockCode + IndicatorCode
```

---

# 13. 数据维护设计

## 13.1 数据归档

长期历史数据：

- 保留；
- 分区；
- 定期归档。

---

## 13.2 数据清理

清理：

- 临时数据；
- 错误同步记录；
- 无效任务日志。

---

# 14. 数据扩展设计

未来支持：

## 更多市场

新增：

```text
Market
```

字段。


支持：

- 港股；
- 美股。


---

## 更多投资模型

新增：

```text
Investment_Model
```

即可扩展：

- PEG模型；
- 彼得林奇模型；
- 趋势模型。

---

# 文档结束

文件名称：

```text
AStockQuant_数据库设计说明书_DD_V1.0.md
```

状态：

Database Design Baseline V1.0