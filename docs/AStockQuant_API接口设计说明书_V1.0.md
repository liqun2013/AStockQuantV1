# AStockQuant API接口设计说明书

**版本：V1.0**  
**项目名称：AStockQuant 股票量化分析系统**  
**文档类型：API Interface Design Document（接口设计说明书）**  
**状态：设计基线版本**  
**更新时间：2026-08-26**

---

# 目录

- [1. 文档说明](#1-文档说明)
- [2. API总体设计](#2-api总体设计)
- [3. API架构设计](#3-api架构设计)
- [4. 接口规范](#4-接口规范)
- [5. 股票基础数据接口](#5-股票基础数据接口)
- [6. 行情数据接口](#6-行情数据接口)
- [7. 财务数据接口](#7-财务数据接口)
- [8. 指标分析接口](#8-指标分析接口)
- [9. 投资模型接口](#9-投资模型接口)
- [10. 股票评分接口](#10-股票评分接口)
- [11. 数据同步接口](#11-数据同步接口)
- [12. 系统管理接口](#12-系统管理接口)
- [13. API安全设计](#13-api安全设计)
- [14. API异常处理](#14-api异常处理)
- [15. API版本管理](#15-api版本管理)

---

# 1. 文档说明

## 1.1 文档目的

本文档用于定义 AStockQuant 系统 API 接口规范。

目标：

- 统一前后端通信标准；
- 定义业务接口；
- 指导 Web 开发；
- 支持未来第三方调用。

---

## 1.2 API定位

AStockQuant API 提供：

- 股票查询；
- 行情查询；
- 财务分析；
- 指标计算；
- 投资模型分析；
- 股票评分。

---

# 2. API总体设计

## 2.1 技术方案

采用：

|项目|方案|
|-|-|
|协议|HTTP/HTTPS|
|风格|RESTful API|
|数据格式|JSON|
|认证|JWT Token|
|框架|ASP.NET Core Web API|

---

# 2.2 API整体架构


```text
             Web Frontend

                  |

             API Controller

                  |

          Application Service

                  |

              Domain

                  |

            Repository

                  |

             SQL Server
```


---

# 3. API架构设计

## 3.1 Controller层

职责：

- 接收请求；
- 参数验证；
- 返回结果。


---

## 3.2 Service层

职责：

- 业务处理；
- 数据组合；
- 调用领域逻辑。


---

## 3.3 Repository层

职责：

- 数据查询；
- 数据保存。


---

# 4. 接口规范

# 4.1 基础URL

开发环境：

```text
https://localhost:5001/api/v1
```

生产环境：

```text
https://api.astockquant.com/api/v1
```

---

# 4.2 请求格式


Header：

```http
Content-Type: application/json
Authorization: Bearer {token}
```

---

# 4.3 返回格式

统一：

```json
{
  "success": true,
  "message": "",
  "data": {}
}
```

---

# 4.4 分页规范


请求：

```json
{
  "pageIndex":1,
  "pageSize":20
}
```


返回：

```json
{
  "total":100,
  "items":[]
}
```

---

# 5. 股票基础数据接口

## 5.1 获取股票列表


接口：

```http
GET /stocks
```


参数：

|参数|说明|
|-|-|
|market|市场|
|industry|行业|


返回：

```json
[
 {
   "code":"600519",
   "name":"贵州茅台",
   "industry":"白酒"
 }
]
```

---

## 5.2 获取股票详情


接口：

```http
GET /stocks/{code}
```


返回：

```json
{
 "code":"600519",
 "name":"贵州茅台",
 "market":"A"
}
```

---

# 6. 行情数据接口

# 6.1 获取日线行情


接口：

```http
GET /stocks/{code}/daily
```


参数：

|参数|说明|
|-|-|
|startDate|开始日期|
|endDate|结束日期|


返回：

```json
[
 {
  "date":"2026-01-01",
  "open":100,
  "close":105,
  "volume":100000
 }
]
```

---

# 6.2 获取实时行情


接口：

```http
GET /stocks/{code}/quote
```


返回：

```json
{
 "price":105,
 "change":2.3,
 "volume":1000000
}
```

---

# 7. 财务数据接口

# 7.1 获取财务报告


接口：

```http
GET /stocks/{code}/financial/report
```


返回：

```json
{
 "revenue":100000000,
 "profit":20000000,
 "roe":18.5
}
```

---

# 7.2 获取财务指标


接口：

```http
GET /stocks/{code}/financial/indicators
```


返回：

```json
{
 "roe":20,
 "grossMargin":45,
 "debtRatio":30
}
```

---

# 8. 指标分析接口

# 8.1 获取股票指标


接口：

```http
GET /stocks/{code}/indicators
```


返回：

```json
{
 "PE":15,
 "PB":2,
 "ROE":18
}
```

---

# 8.2 重新计算指标


接口：

```http
POST /stocks/{code}/indicators/calculate
```


用途：

手动触发指标计算。

---

# 9. 投资模型接口


# 9.1 获取模型列表


接口：

```http
GET /models
```


返回：

```json
[
 {
  "code":"FISHER",
  "name":"Fisher成长模型"
 }
]
```

---

# 9.2 执行投资模型分析


接口：

```http
POST /models/{code}/analysis
```


参数：

```json
{
 "stockCode":"600519"
}
```


返回：

```json
{
 "score":90,
 "level":"A"
}
```

---

# 10. 股票评分接口


# 10.1 获取股票综合评分


接口：

```http
GET /stocks/{code}/score
```


返回：

```json
{
 "fisher":90,
 "buffett":85,
 "graham":80,
 "total":85
}
```

---

# 10.2 获取股票排行榜


接口：

```http
GET /ranking
```


参数：

|参数|说明|
|-|-|
|model|模型|
|limit|数量|


返回：

```json
[
 {
 "code":"600519",
 "score":95
 }
]
```

---

# 11. 数据同步接口


# 11.1 启动数据同步


接口：

```http
POST /sync/start
```


参数：

```json
{
"type":"daily"
}
```


---

# 11.2 查询同步状态


接口：

```http
GET /sync/status
```


返回：

```json
{
"status":"running",
"progress":80
}
```

---

# 12. 系统管理接口


## 12.1 健康检查


接口：

```http
GET /health
```


返回：

```json
{
"status":"ok"
}
```

---

## 12.2 系统信息


接口：

```http
GET /system/info
```

---

# 13. API安全设计


## 13.1 身份认证

采用：

JWT Token。


流程：

```text
Login

 |

Token

 |

API Request

 |

Validate
```

---

## 13.2 权限控制


角色：

- Admin；
- Analyst；
- Viewer。


---

# 14. API异常处理


统一错误格式：

```json
{
 "success":false,
 "errorCode":"STOCK_NOT_FOUND",
 "message":"股票不存在"
}
```

---

错误分类：

|类型|Code|
|-|-|
|参数错误|400|
|未授权|401|
|无权限|403|
|系统错误|500|

---

# 15. API版本管理


当前版本：

```text
/api/v1
```


未来：

```text
/api/v2
```


版本策略：

- 新增接口兼容；
- 删除接口升级版本；
- 保留历史版本。

---

# 文档结束


文件名称：

```text
AStockQuant_API接口设计说明书_V1.0.md
```

状态：

API Design Baseline V1.0