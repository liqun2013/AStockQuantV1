AStockQuant 数据流程设计说明书

版本：V1.0
项目名称：AStockQuant 股票量化分析系统
文档类型：Data Flow Design Document（数据流程设计说明书）
状态：设计基线版本
更新时间：2026-08-26

目录
1. 文档说明
2. 数据体系总体设计
3. 数据架构设计
4. 数据生命周期设计
5. 数据采集流程设计
6. 行情数据流程设计
7. 财务数据流程设计
8. 数据清洗与标准化流程
9. 数据入库流程设计
10. 指标计算数据流程
11. 投资模型数据流程
12. 股票评分流程设计
13. 数据质量控制设计
14. 数据同步任务流程
15. 数据查询流程设计
16. 数据备份与恢复设计
17. 数据扩展设计
1. 文档说明
1.1 文档目的

本文档描述 AStockQuant 系统内部数据流转过程。

目标：

明确数据来源；
明确数据处理过程；
明确数据存储方式；
明确数据计算链路；
保证数据可追踪。
1.2 数据设计原则

系统遵循：

原始数据保留原则

不直接修改外部数据。

流程：

外部数据

↓

Raw Data

↓

标准化数据

↓

分析结果
数据可追溯原则

任何评分结果必须能够追溯：

评分结果

↓

指标结果

↓

财务数据

↓

原始数据
数据分层原则

数据分为：

原始数据层
标准数据层
计算数据层
分析结果层
2. 数据体系总体设计

AStockQuant 数据体系：

                 外部数据源

                     |
        --------------------------------

        AKTools / AKShare / 市场数据接口

                     |

              数据采集层

                     |

              数据处理层

                     |

              数据存储层

                     |

             指标计算引擎

                     |

             投资分析模型

                     |

             用户查询展示
3. 数据架构设计
3.1 数据分层模型
Layer 1：数据源层

来源：

AKTools；
AKShare；
东方财富；
交易所公开数据。

数据类型：

行情；
财务；
估值；
行业。
Layer 2：ODS 原始数据层

作用：

保存：

原始接口返回数据；
原始JSON；
原始字段。

目的：

保证：

数据来源可复现。

Layer 3：业务数据层

存储：

股票信息；
行情数据；
财务数据；
指标数据。
Layer 4：分析结果层

存储：

股票评分；
策略结果；
推荐列表。
4. 数据生命周期设计

完整生命周期：

采集

↓

验证

↓

清洗

↓

转换

↓

入库

↓

计算

↓

评分

↓

展示

↓

归档
5. 数据采集流程设计
5.1 总体流程
Scheduler

    |

Data Provider

    |

AKTools API

    |

External Market Data

    |

Data Transfer Object

    |

Data Processing
5.2 数据采集组件

主要组件：

组件	职责
Scheduler	触发任务
DataProvider	调用接口
Mapper	数据转换
Validator	数据校验
Repository	保存数据
6. 行情数据流程设计
6.1 日线行情流程
交易市场

↓

AKTools

↓

StockDailyDTO

↓

行情校验

↓

StockDailyPrice

↓

SQL Server
6.2 数据校验

检查：

股票代码是否合法；
日期是否重复；
收盘价是否为空；
成交量是否异常。
7. 财务数据流程设计
7.1 财报采集流程
上市公司财报

↓

AKTools

↓

Financial DTO

↓

字段转换

↓

Financial Statement

↓

Database
7.2 财务数据类型

包括：

利润表

字段：

营业收入；
净利润；
毛利率。
资产负债表

字段：

总资产；
总负债；
股东权益。
现金流量表

字段：

经营现金流；
投资现金流；
自由现金流。
8. 数据清洗与标准化流程
8.1 清洗流程
原始数据

↓

格式检查

↓

字段映射

↓

异常过滤

↓

标准数据
8.2 标准化内容

统一：

股票代码格式；
日期格式；
金额单位；
百分比格式。
9. 数据入库流程设计
9.1 入库流程
DTO

↓

Entity

↓

Repository

↓

Transaction

↓

SQL Server
9.2 入库策略

采用：

增量更新

适用于：

日行情；
财务指标。
全量同步

适用于：

股票列表；
行业分类。
10. 指标计算数据流程
10.1 指标计算流程
基础数据

↓

Indicator Engine

↓

指标结果表

↓

Score Engine
10.2 指标来源

例如：

ROE：

净利润

/

平均净资产

成长率：

今年收入

/

去年收入 -1
11. 投资模型数据流程
11.1 Fisher模型

输入：

财务增长数据；
行业数据；
盈利能力。

流程：

Financial Data

↓

Growth Indicator

↓

Fisher Rule

↓

Fisher Score
11.2 Buffett模型

输入：

ROE；
现金流；
护城河指标。

流程：

Company Quality Data

↓

Buffett Engine

↓

Buffett Score
11.3 Graham模型

输入：

PE；
PB；
股息率。

流程：

Valuation Data

↓

Margin Safety Rule

↓

Graham Score
12. 股票评分流程设计

综合评分流程：

Fisher Score

+

Buffett Score

+

Graham Score


↓

Total Score


↓

Ranking

↓

Investment Candidate List
13. 数据质量控制设计
13.1 数据完整性

检查：

数据是否缺失；
数据是否重复；
数据时间连续性。
13.2 数据异常检测

例如：

股价：

昨日100

今日10000

异常

处理：

标记；
不参与计算。
14. 数据同步任务流程

每日任务：

09:00

更新基础数据


15:30

获取收盘行情


18:00

同步财务数据


20:00

执行指标计算


21:00

生成股票排名
15. 数据查询流程设计

用户查询：

Web

↓

Application Service

↓

Repository

↓

Database

↓

DTO

↓

页面展示
16. 数据备份与恢复设计
备份对象

包括：

SQL Server数据库；
配置文件；
分析结果。
恢复流程
Backup File

↓

Restore Database

↓

Data Validation

↓

System Recovery
17. 数据扩展设计

未来支持：

更多市场

例如：

港股；
美股。
更多数据源

例如：

Wind；
同花顺；
聚宽。
AI数据分析

未来增加：

财报文本

↓

AI模型

↓

风险分析

↓

投资建议