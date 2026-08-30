AStockQuant 产品需求规格说明书（PRD + SRS）

版本：

AStockQuant V2.0
Investment Quantitative Analysis Platform

文档版本：
V1.0

数据库：
SQL Server 2019

技术方向：
.NET 10 + ASP.NET Core + Dapper + AKTools

1. 项目概述

1.1 项目名称

AStockQuant

中文：

A股价值投资量化分析系统

1.2 项目定位

AStockQuant 是一个面向个人投资者的：

基于 Buffett + Graham + Fisher 投资理念，同时结合量化分析、财务分析、估值分析、风险控制和历史回测的 A 股投资研究平台。

核心目标：

不是预测股票短期涨跌。

而是：

寻找优秀企业
+
合理价格买入
+
长期跟踪企业变化
+
辅助投资决策

2. 核心投资理念

系统基于三个投资流派。

2.1 Buffett 模型（企业质量）

核心问题：

这是不是一家优秀企业？

关注：

盈利能力

指标：

ROE
ROIC
ROA
净利润率
毛利率
盈利稳定性

指标：

连续盈利年份

利润波动率

利润增长稳定性
现金创造能力

指标：

经营现金流

自由现金流

FCF Margin

现金流/净利润
财务安全

指标：

资产负债率

有息负债率

流动比率

现金覆盖债务
Buffett评分输出

例如：

90-100
优秀企业

75-90
良好企业

60-75
一般

<60
不符合价值投资标准

2.2 Graham 模型（价值）

核心问题：

当前价格是否便宜？

关注：

估值指标
PE

PB

PS

EV/EBITDA
安全边际

计算：

内在价值

市场价格

安全边际

公式：

Margin Of Safety

=
(Intrinsic Value - Market Price)
/ Intrinsic Value
Graham评分输出

例如：

90+
严重低估

75-90
合理偏低

60-75
合理

<60
高估

2.3 Fisher 模型（成长）

核心问题：

企业未来有没有持续成长能力？

关注：

收入增长
Revenue CAGR 3Y

Revenue CAGR 5Y

Revenue CAGR 10Y
利润增长
Profit CAGR

EPS CAGR
竞争优势

指标：

毛利率稳定

ROIC

市场份额

研发投入
管理质量

指标：

资本配置能力

并购能力

分红能力

3. 系统总体功能

系统分为：

数据采集层

↓

数据存储层

↓

指标计算层

↓

投资模型层

↓

策略分析层

↓

投资组合管理层

4. 功能模块详细需求

Module 01：股票基础数据管理
功能

维护 A 股股票基础信息。

来源：

AKShare
AKTools
数据包括

股票：

代码

名称

交易所

上市日期

行业

市场类型
支持
查询

例如：

查询所有沪深300股票

查询新能源行业股票

查询上市超过10年的公司

Module 02：行情数据中心
功能

管理每日行情。

数据：

日K

周K

月K

字段：

开盘

最高

最低

收盘

成交量

成交额

换手率

市值

支持：

技术分析

例如：

MA

MACD

RSI

KDJ

波动率

最大回撤

Module 03：财务数据中心

核心模块。

财务报表

包括：

利润表
营业收入

营业成本

净利润

EPS
资产负债表
资产

负债

权益

现金
现金流量表
经营现金流

投资现金流

融资现金流

自由现金流
财务指标

自动计算：

ROE

ROIC

毛利率

净利率

资产负债率

现金流质量

Module 04：长期能力分析

对应：

Finance.LongTermMetric

计算：

3年
收入 CAGR

利润 CAGR

ROE 平均值
5年
收入 CAGR

利润 CAGR

ROIC

FCF稳定性
10年
长期竞争力

盈利稳定性

用途：

Buffett/Fisher评分。

Module 05：估值分析系统

支持：

PE估值

包括：

静态PE

TTM PE

Forward PE

历史百分位
PB估值

包括：

PB

历史PB区间
DCF估值

支持：

输入：

未来收入增长

利润率

折现率

终值增长率

输出：

企业价值

股票价值

安全边际
Graham估值

支持：

Graham Number

保守估值

安全价格

Module 06：行业分析

解决：

不同行业不能用同一套规则。

支持行业：

银行

保险

券商

消费

科技

制造

周期

资源

公用事业

例如：

银行：

重点：

ROE

PB

净息差

不良率

资源：

重点：

现金流

资源储量

商品周期

股息

Module 07：投资评分系统

核心模块。

输入：

Buffett Score

+

Graham Score

+

Fisher Score

+

Valuation Score

+

Risk Score

输出：

Final Score

评分模型：

默认：

Buffett        30%

Graham         25%

Fisher         25%

Valuation     15%

Risk           5%

输出等级：

A+

优秀投资机会

A

值得研究

B

观察

C

谨慎

D

不推荐

Module 08：股票筛选器

类似：

“价值投资雷达”。

支持条件：

例如：

Buffett筛选
ROE >15%

连续盈利10年

FCF为正

负债低
Graham筛选
PE < 行业平均

PB < 历史50%

安全边际>30%
Fisher筛选
收入增长>15%

利润增长>15%

ROIC提升

组合条件：

例如：

ROE >15%

AND

PE <20

AND

5年利润增长>10%

AND

FinalScore >80

Module 09：策略回测系统

核心目标：

验证投资逻辑。

支持：

历史选股

例如：

2018-01-01

按照当时数据

选择Top50股票

禁止：

未来数据。

必须：

PublishDate <= TradeDate

回测指标：

收益率

年化收益

最大回撤

夏普比率

胜率

换手率

Module 10：投资组合管理

管理真实投资。

支持：

账户：

现金

股票

成本

收益

交易记录：

买入

卖出

分红

手续费

组合分析：

收益曲线

行业分布

风险暴露

Module 11：数据导入系统

对应 AKTools。

支持：

任务：

每日行情更新

财报更新

估值更新

指标计算

状态：

成功

失败

部分成功

记录：

耗时

数量

错误信息

Module 12：AI投资助手（未来）

未来扩展。

能力：

输入：

600519 贵州茅台

自动生成：

企业分析

估值分析

风险分析

投资观点

5. 非功能需求
性能要求
数据规模

设计支持：

5000+ 股票

10年以上历史行情

20年以上财报
查询性能

排行榜：

目标：

<2秒

股票详情：

目标：

<1秒
可靠性

要求：

数据导入可恢复

失败可重试

日志完整
安全

支持：

用户认证

权限控制

操作日志

6. 系统技术架构

最终：

                Web UI
                  |
                  |
             ASP.NET Core API
                  |
        -----------------------
        |                     |
 Application Layer       Background Jobs
        |
 Domain Layer
        |
 Infrastructure
        |
 ---------------------
 |                   |
SQL Server       AKTools API

7. .NET 项目结构规划

最终不要继续沿用之前混乱结构。

重新定义：

AStockQuant.sln

src

├── AStockQuant.Domain

├── AStockQuant.Application

├── AStockQuant.Infrastructure

├── AStockQuant.Data

├── AStockQuant.WebApi

├── AStockQuant.Worker

└── AStockQuant.Tests

8. 开发阶段规划

Phase 0

基础工程

完成：

Solution

DI

Configuration

Logging

Exception Handling

Phase 1

数据中心

完成：

AKTools Client

Stock Import

Daily Price Import

Financial Import

Phase 2

财务分析

完成：

FinancialIndicator

LongTermMetric

Valuation

Phase 3

投资模型

完成：

Buffett

Graham

Fisher

Score Engine

Phase 4

策略系统

完成：

Stock Screening

Backtest

Ranking

Phase 5

投资组合

完成：

Portfolio

Trade

Performance

9. 项目最终目标

AStockQuant 最终成为：

一个个人版 Morningstar + Value Line + QuantConnect 的 A 股价值投资研究平台。

核心能力：

找到好公司
        +
判断合理价格
        +
长期跟踪变化
        +
验证投资方法
        +
辅助实际投资