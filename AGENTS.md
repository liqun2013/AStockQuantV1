# \# AStockQuant Development Guide

# 

# \## Project Overview

# 

# AStockQuant is an A-share quantitative investment research platform.

# 

# Core ideas:

# 

# \- Fisher growth investing

# \- Buffett quality investing

# \- Graham value investing

# 

# 

# \## Technology Stack

# 

# \- .NET 10

# \- ASP.NET Core

# \- SQL Server 2019

# \- Dapper

# \- AKTools

# \- Bootstrap

# 

# 

# \## Architecture Rules

# 

# \- Domain project must remain independent.

# \- Do not use Entity Framework Core.

# \- Database access belongs to Infrastructure.

# \- Business logic belongs to Application.

# \- Web layer cannot directly access database.

# 

# 

# \## Coding Rules

# 

# \- All code identifiers must use English.

# \- Chinese is allowed only in:

# &#x20; - comments

# &#x20; - UI display text

# &#x20; - documentation

# 

# 

# \## Database Rules

# 

# \- SQL Server 2019 compatible.

# \- Prefer batch operations.

# \- Avoid row-by-row insert for large datasets.

# \- Use indexes for StockCode + TradeDate queries.

# 

# 

# \## Quant Rules

# 

# Factor calculation must be deterministic.

# 

# Historical backtest must avoid future data leakage.

# 

# 

# \## Testing Rules

# 

# Every major module should include unit tests.

# 

