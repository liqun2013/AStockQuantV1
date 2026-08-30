# AStockQuant 部署架构设计说明书

**版本：V1.0**  
**项目名称：AStockQuant 股票量化分析系统**  
**文档类型：Deployment Architecture Design Document（部署架构设计说明书）**  
**状态：设计基线版本**  
**更新时间：2026-08-26**

---

# 目录

- [1. 文档说明](#1-文档说明)
- [2. 部署总体设计](#2-部署总体设计)
- [3. 环境规划设计](#3-环境规划设计)
- [4. 系统部署架构](#4-系统部署架构)
- [5. Docker部署设计](#5-docker部署设计)
- [6. 应用服务部署设计](#6-应用服务部署设计)
- [7. AKTools服务部署设计](#7-aktools服务部署设计)
- [8. SQL Server部署设计](#8-sql-server部署设计)
- [9. 配置管理设计](#9-配置管理设计)
- [10. 日志管理设计](#10-日志管理设计)
- [11. 数据备份设计](#11-数据备份设计)
- [12. 发布流程设计](#12-发布流程设计)
- [13. CI/CD设计](#13-cicd设计)
- [14. 运维监控设计](#14-运维监控设计)
- [15. 高可用扩展设计](#15-高可用扩展设计)

---

# 1. 文档说明

## 1.1 文档目的

本文档定义 AStockQuant 系统部署架构。

目标：

- 统一部署方式；
- 降低环境差异；
- 支持开发、测试、生产环境；
- 支持未来云部署。

---

# 2. 部署总体设计

## 2.1 部署目标

AStockQuant采用：

> Docker容器化 + 服务分离 + 配置外置化

实现：

- 快速部署；
- 环境一致；
- 易于扩展。


---

# 2.2 总体部署架构


```text
                    用户

                     |

                 Browser

                     |

             AStockQuant.Web

                     |

        -------------------------

        |                       |

 Application API          Worker Service

        |                       |

        -------------------------

                     |

              SQL Server 2019

                     |

              AKTools Container

                     |

                 AKShare
```


---

# 3. 环境规划设计


系统分为：

|环境|用途|
|-|-|
|Development|本地开发|
|Testing|测试验证|
|Production|正式运行|

---

# 3.1 开发环境


要求：

- Windows/Linux；
- .NET 10 SDK；
- Docker Desktop；
- SQL Server。


---

# 3.2 测试环境


特点：

- 独立数据库；
- 独立AKTools；
- 测试数据。


---

# 3.3 生产环境


建议：

服务器：

- Linux；
- Docker；
- SQL Server。


---

# 4. 系统部署架构


## 4.1 服务拆分


系统服务：


```text
AStockQuant.Web

职责：

Web/API接口


----------------


AStockQuant.Worker

职责：

后台任务


----------------


AKTools

职责：

股票数据接口


----------------


SQL Server

职责：

业务数据存储
```


---

# 5. Docker部署设计


## 5.1 容器规划


推荐：


```text
docker-compose.yml


services:

  astockquant-web

  astockquant-worker

  aktools

  sqlserver
```


---

# 5.2 网络设计


容器网络：

```text
astockquant-network
```


服务通信：

```text
Web

|

Worker

|

AKTools

|

SQL Server
```


---

# 5.3 数据持久化


数据卷：


```text
Volumes:

sqlserver-data

app-logs

aktools-cache
```


---

# 6. 应用服务部署设计


# 6.1 Web服务


部署：

```text
AStockQuant.Web.dll
```


运行：

```text
ASP.NET Core Runtime
```


职责：

- API；
- 页面；
- 用户访问。


---

# 6.2 Worker服务


部署：

```text
AStockQuant.Worker.dll
```


职责：

- 定时同步；
- 指标计算；
- 排名生成。


---

# 7. AKTools服务部署设计


## 7.1 服务说明


AKTools作为：

> AStockQuant外部股票数据服务层


架构：


```text
AStockQuant

        |

HTTP

        |

AKTools

        |

AKShare
```


---

# 7.2 AKTools配置


配置：

包括：

- 服务地址；
- 超时时间；
- 重试次数。


---

# 7.3 异常处理


当AKTools异常：

流程：

```text
API失败

↓

记录日志

↓

自动重试

↓

失败告警
```


---

# 8. SQL Server部署设计


## 8.1 数据库要求


版本：

```text
SQL Server 2019+
```


---

## 8.2 数据库目录


建议：

```text
Database

 ├── Data

 ├── Log

 └── Backup
```


---

# 8.3 数据库优化


包括：

- 索引维护；
- 数据备份；
- SQL监控；
- 定期统计信息更新。


---

# 9. 配置管理设计


## 9.1 配置原则


禁止：

代码中保存：

- 数据库密码；
- API地址；
- Token。


---

## 9.2 配置文件


结构：


```text
appsettings.json

appsettings.Development.json

appsettings.Production.json
```


---

## 9.3 环境变量


生产环境：

```text
ConnectionStrings__Default
AKTools__Url
Jwt__Secret
```


---

# 10. 日志管理设计


## 10.1 日志分类


包括：

### 应用日志

记录：

- 请求；
- 异常。


### 数据同步日志

记录：

- 同步任务；
- 数据量。


### 计算日志

记录：

- 指标计算；
- 模型运行。


---

# 10.2 日志存储


方式：

- 文件；
- 数据库；
- 日志平台。


---

# 11. 数据备份设计


## 11.1 数据库备份策略


建议：

每日：

- 完整备份。


每小时：

- 日志备份。


---

## 11.2 备份内容


包括：

- SQL Server数据库；
- 配置文件；
- Docker配置。


---

# 12. 发布流程设计


流程：


```text
开发

 |

Git Commit

 |

CI Build

 |

Test

 |

Docker Image

 |

Deploy

 |

Verify
```


---

# 13. CI/CD设计


## 13.1 GitHub Actions流程


```text
Push

 |

Restore

 |

Build

 |

Test

 |

Docker Build

 |

Deploy
```


---

## 13.2 镜像管理


镜像：

```text
astockquant-web

astockquant-worker

astockquant-aktools
```


---

# 14. 运维监控设计


监控：

## 服务状态

检查：

- Web是否运行；
- Worker是否运行；
- AKTools是否可用。


---

## 数据状态


检查：

- 最近同步时间；
- 股票数量；
- 数据异常。


---

## 性能状态


监控：

- CPU；
- 内存；
- 数据库空间。


---

# 15. 高可用扩展设计


未来支持：


## Web水平扩展


```text
Load Balancer

      |

-----------------

Web1

Web2

Web3
```


---

## 数据库扩展


支持：

- 主从复制；
- 读写分离；
- 数据仓库。


---

## 任务扩展


Worker支持：

- 多实例；
- 分布式任务调度。


---

# 文档结束


文件名称：

```text
AStockQuant_部署架构设计说明书_V1.0.md
```

状态：

Deployment Architecture Baseline V1.0