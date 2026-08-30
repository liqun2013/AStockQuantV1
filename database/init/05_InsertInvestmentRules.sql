/*
====================================================
 05_InsertInvestmentRules.sql
 Investment Model Rules
 SQL Server 2019
====================================================
*/

USE AStockQuant;
GO

/*
====================================================
 Buffett Indicators
====================================================
*/

INSERT INTO Buffett.Indicator
(
    IndicatorCode,
    IndicatorName,
    Category,
    Weight,
    MaxScore,
    Description,
    IsQuantitative
)
VALUES
(
    'B01',
    N'ROE',
    'Quality',
    20,
    10,
    N'长期股东权益回报能力，重点观察多年平均ROE及稳定性。',
    1
),
(
    'B02',
    N'ROIC',
    'Quality',
    20,
    10,
    N'投入资本回报率，用于判断企业真实资本效率。',
    1
),
(
    'B03',
    N'自由现金流',
    'CashFlow',
    15,
    10,
    N'经营现金流减资本开支，衡量企业实际现金创造能力。',
    1
),
(
    'B04',
    N'自由现金流稳定性',
    'CashFlow',
    10,
    10,
    N'观察过去多年自由现金流为正及稳定增长的能力。',
    1
),
(
    'B05',
    N'毛利率',
    'Quality',
    10,
    10,
    N'衡量产品定价能力和商业模式质量。',
    1
),
(
    'B06',
    N'净利率',
    'Quality',
    5,
    10,
    N'衡量企业最终盈利效率。',
    1
),
(
    'B07',
    N'资本配置能力',
    'Management',
    10,
    10,
    N'综合评价分红、回购、再投资及并购等资本配置能力。',
    0
),
(
    'B08',
    N'护城河',
    'Moat',
    10,
    10,
    N'评价品牌、成本优势、网络效应、规模优势、牌照等长期竞争优势。',
    0
);
GO

--INSERT INTO Buffett.ScoreRule
--(
--    IndicatorId,
--    MinValue,
--    MaxValue,
--    Score,
--    RuleOrder,
--    Description
--)
--SELECT
--    IndicatorId,
--    25,
--    NULL,
--    10,
--    1,
--    N'ROE >= 25%'
--FROM Buffett.Indicator
--WHERE IndicatorCode = 'B01';

--INSERT INTO Buffett.ScoreRule
--SELECT
--    IndicatorId, 20, 25, 9, 2, N'20% <= ROE < 25%',
--    1, SYSUTCDATETIME()
--FROM Buffett.Indicator
--WHERE IndicatorCode = 'B01';

--INSERT INTO Buffett.ScoreRule
--(
--    IndicatorId, MinValue, MaxValue, Score, RuleOrder, Description
--)
--SELECT
--    IndicatorId, 15, 20, 7, 3, N'15% <= ROE < 20%'
--FROM Buffett.Indicator
--WHERE IndicatorCode = 'B01';

--INSERT INTO Buffett.ScoreRule
--SELECT
--    IndicatorId, 10, 15, 5, 4, N'10% <= ROE < 15%',
--    1, SYSUTCDATETIME()
--FROM Buffett.Indicator
--WHERE IndicatorCode = 'B01';

--INSERT INTO Buffett.ScoreRule
--SELECT
--    IndicatorId, 5, 10, 2, 5, N'5% <= ROE < 10%',
--    1, SYSUTCDATETIME()
--FROM Buffett.Indicator
--WHERE IndicatorCode = 'B01';

--INSERT INTO Buffett.ScoreRule
--SELECT
--    IndicatorId, NULL, 5, 0, 6, N'ROE < 5%',
--    1, SYSUTCDATETIME()
--FROM Buffett.Indicator
--WHERE IndicatorCode = 'B01';
--GO

---Buffett 评分规则
DELETE FROM Buffett.ScoreRule
WHERE IndicatorId =
(
    SELECT IndicatorId
    FROM Buffett.Indicator
    WHERE IndicatorCode = 'B01'
);
GO

INSERT INTO Buffett.ScoreRule
(
    IndicatorId,
    MinValue,
    MaxValue,
    Score,
    RuleOrder,
    Description,
    IsActive,
    CreatedTime
)
SELECT IndicatorId, 25, NULL, 10, 1, N'ROE >= 25%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B01'
UNION ALL
SELECT IndicatorId, 20, 25, 9, 2, N'20% <= ROE < 25%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B01'
UNION ALL
SELECT IndicatorId, 15, 20, 7, 3, N'15% <= ROE < 20%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B01'
UNION ALL
SELECT IndicatorId, 10, 15, 5, 4, N'10% <= ROE < 15%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B01'
UNION ALL
SELECT IndicatorId, 5, 10, 2, 5, N'5% <= ROE < 10%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B01'
UNION ALL
SELECT IndicatorId, NULL, 5, 0, 6, N'ROE < 5%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B01';
GO
--ROIC
INSERT INTO Buffett.ScoreRule
(
    IndicatorId,
    MinValue,
    MaxValue,
    Score,
    RuleOrder,
    Description,
    IsActive,
    CreatedTime
)
SELECT IndicatorId, 20, NULL, 10, 1, N'ROIC >= 20%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B02'
UNION ALL
SELECT IndicatorId, 15, 20, 8, 2, N'15% <= ROIC < 20%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B02'
UNION ALL
SELECT IndicatorId, 10, 15, 6, 3, N'10% <= ROIC < 15%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B02'
UNION ALL
SELECT IndicatorId, 5, 10, 3, 4, N'5% <= ROIC < 10%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B02'
UNION ALL
SELECT IndicatorId, NULL, 5, 0, 5, N'ROIC < 5%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B02';
GO
--自由现金流
INSERT INTO Buffett.ScoreRule
(
    IndicatorId,
    MinValue,
    MaxValue,
    Score,
    RuleOrder,
    Description,
    IsActive,
    CreatedTime
)
SELECT IndicatorId, 20, NULL, 10, 1, N'FCF Margin >= 20%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B03'
UNION ALL
SELECT IndicatorId, 15, 20, 8, 2, N'15% <= FCF Margin < 20%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B03'
UNION ALL
SELECT IndicatorId, 10, 15, 6, 3, N'10% <= FCF Margin < 15%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B03'
UNION ALL
SELECT IndicatorId, 5, 10, 3, 4, N'5% <= FCF Margin < 10%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B03'
UNION ALL
SELECT IndicatorId, NULL, 5, 0, 5, N'FCF Margin < 5%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B03'
GO
--FCF 稳定性
INSERT INTO Buffett.ScoreRule
(
    IndicatorId,
    MinValue,
    MaxValue,
    Score,
    RuleOrder,
    Description,
    IsActive,
    CreatedTime
)
SELECT IndicatorId, 1, 1.000001, 10, 1, N'5年FCF全部为正', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B04'
UNION ALL
SELECT IndicatorId, 0.8, 1, 8, 2, N'5年FCF至少80%为正', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B04'
UNION ALL
SELECT IndicatorId, 0.6, 0.8, 6, 3, N'5年FCF至少60%为正', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B04'
UNION ALL
SELECT IndicatorId, 0.4, 0.6, 3, 4, N'5年FCF至少40%为正', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B04'
UNION ALL
SELECT IndicatorId, NULL, 0.4, 0, 5, N'5年FCF稳定性不足40%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B04'
GO
--毛利率
INSERT INTO Buffett.ScoreRule
(
    IndicatorId,
    MinValue,
    MaxValue,
    Score,
    RuleOrder,
    Description,
    IsActive,
    CreatedTime
)
SELECT IndicatorId, 60, NULL, 10, 1, N'Gross Margin >= 60%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B05'
UNION ALL
SELECT IndicatorId, 40, 60, 8, 2, N'40% <= Gross Margin < 60%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B05'
UNION ALL
SELECT IndicatorId, 30, 40, 6, 3, N'30% <= Gross Margin < 40%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B05'
UNION ALL
SELECT IndicatorId, 20, 30, 4, 4, N'20% <= Gross Margin < 30%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B05'
UNION ALL
SELECT IndicatorId, 10, 20, 2, 5, N'10% <= Gross Margin < 20%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B05'
UNION ALL
SELECT IndicatorId, NULL, 10, 0, 6, N'Gross Margin < 10%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B05'
GO
--净利率
INSERT INTO Buffett.ScoreRule
(
    IndicatorId,
    MinValue,
    MaxValue,
    Score,
    RuleOrder,
    Description,
    IsActive,
    CreatedTime
)
SELECT IndicatorId, 25, NULL, 10, 1, N'Net Margin >= 25%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B06'
UNION ALL
SELECT IndicatorId, 15, 25, 8, 2, N'15% <= Net Margin < 25%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B06'
UNION ALL
SELECT IndicatorId, 10, 15, 6, 3, N'10% <= Net Margin < 15%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B06'
UNION ALL
SELECT IndicatorId, 5, 10, 3, 4, N'5% <= Net Margin < 10%', 1, SYSUTCDATETIME()
FROM Buffett.Indicator WHERE IndicatorCode = 'B06'
UNION ALL
SELECT IndicatorId, NULL, 5, 0, 5, N'Net Margin < 5%', 1, SYSUTCDATETIME();
GO

------Graham 指标初始化
/*
====================================================
 Graham Indicators
====================================================
*/

INSERT INTO Graham.Indicator
(
    IndicatorCode,
    IndicatorName,
    Category,
    Weight,
    MaxScore,
    Description,
    IsQuantitative
)
VALUES
(
    'G01',
    N'PE',
    'Valuation',
    15,
    10,
    N'市盈率水平。',
    1
),
(
    'G02',
    N'PB',
    'Valuation',
    10,
    10,
    N'市净率水平。',
    1
),
(
    'G03',
    N'Graham Number',
    'Valuation',
    15,
    10,
    N'基于EPS和BVPS的传统Graham估值。',
    1
),
(
    'G04',
    N'安全边际',
    'SafetyMargin',
    20,
    10,
    N'当前价格相对于估值的折价程度。',
    1
),
(
    'G05',
    N'财务安全',
    'FinancialSafety',
    15,
    10,
    N'负债和资产安全程度。',
    1
),
(
    'G06',
    N'流动性',
    'FinancialSafety',
    5,
    10,
    N'流动资产对短期负债的覆盖能力。',
    1
),
(
    'G07',
    N'盈利稳定性',
    'Earnings',
    10,
    10,
    N'多年持续盈利能力。',
    1
),
(
    'G08',
    N'分红记录',
    'ShareholderReturn',
    10,
    10,
    N'长期稳定分红记录。',
    1
);
GO
-----------Graham 评分规则
INSERT INTO Graham.ScoreRule
(
    IndicatorId,
    MinValue,
    MaxValue,
    Score,
    RuleOrder,
    Description,
    IsActive,
    CreatedTime
)
SELECT IndicatorId, NULL, 10, 10, 1, N'PE <= 10', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G01'
UNION ALL
SELECT IndicatorId, 10, 15, 8, 2, N'10 < PE <= 15', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G01'
UNION ALL
SELECT IndicatorId, 15, 20, 6, 3, N'15 < PE <= 20', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G01'
UNION ALL
SELECT IndicatorId, 20, 30, 3, 4, N'20 < PE <= 30', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G01'
UNION ALL
SELECT IndicatorId, 30, NULL, 0, 5, N'PE > 30', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G01'
GO
------PB
INSERT INTO Graham.ScoreRule
(
    IndicatorId,
    MinValue,
    MaxValue,
    Score,
    RuleOrder,
    Description,
    IsActive,
    CreatedTime
)
SELECT IndicatorId, NULL, 1.5, 10, 1, N'PB <= 1.5', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G02'
UNION ALL
SELECT IndicatorId, 1.5, 2, 8, 2, N'1.5 < PB <= 2', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G02'
UNION ALL
SELECT IndicatorId, 2, 3, 6, 3, N'2 < PB <= 3', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G02'
UNION ALL
SELECT IndicatorId, 3, 5, 3, 4, N'3 < PB <= 5', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G02'
UNION ALL
SELECT IndicatorId, 5, NULL, 0, 5, N'PB > 5', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G02'
GO
---Graham Number
INSERT INTO Graham.ScoreRule
(
    IndicatorId,
    MinValue,
    MaxValue,
    Score,
    RuleOrder,
    Description,
    IsActive,
    CreatedTime
)
SELECT IndicatorId, 1.5, NULL, 10, 1, N'Graham Value / Price >= 1.5', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G03'
UNION ALL
SELECT IndicatorId, 1.25, 1.5, 8, 2, N'1.25 <= Graham Value / Price < 1.5', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G03'
UNION ALL
SELECT IndicatorId, 1.0, 1.25, 6, 3, N'1.0 <= Graham Value / Price < 1.25', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G03'
UNION ALL
SELECT IndicatorId, 0.8, 1.0, 3, 4, N'0.8 <= Graham Value / Price < 1.0', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G03'
UNION ALL
SELECT IndicatorId, NULL, 0.8, 0, 5, N'Graham Value / Price < 0.8', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G03'
GO
---安全边际
INSERT INTO Graham.ScoreRule
(
    IndicatorId,
    MinValue,
    MaxValue,
    Score,
    RuleOrder,
    Description,
    IsActive,
    CreatedTime
)
SELECT IndicatorId, 0.50, NULL, 10, 1, N'安全边际 >= 50%', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G04'
UNION ALL
SELECT IndicatorId, 0.30, 0.50, 8, 2, N'30% <= 安全边际 < 50%', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G04'
UNION ALL
SELECT IndicatorId, 0.20, 0.30, 6, 3, N'20% <= 安全边际 < 30%', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G04'
UNION ALL
SELECT IndicatorId, 0.10, 0.20, 3, 4, N'10% <= 安全边际 < 20%', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G04'
UNION ALL
SELECT IndicatorId, NULL, 0.10, 0, 5, N'安全边际 < 10%', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G04'
GO
--财务安全
INSERT INTO Graham.ScoreRule
(
    IndicatorId,
    MinValue,
    MaxValue,
    Score,
    RuleOrder,
    Description,
    IsActive,
    CreatedTime
)
SELECT IndicatorId, NULL, 30, 10, 1, N'资产负债率 < 30%', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G05'
UNION ALL
SELECT IndicatorId, 30, 50, 8, 2, N'30% <= 资产负债率 < 50%', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G05'
UNION ALL
SELECT IndicatorId, 50, 60, 6, 3, N'50% <= 资产负债率 < 60%', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G05'
UNION ALL
SELECT IndicatorId, 60, 70, 3, 4, N'60% <= 资产负债率 < 70%', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G05'
UNION ALL
SELECT IndicatorId, 70, NULL, 0, 5, N'资产负债率 >= 70%', 1, SYSUTCDATETIME()
FROM Graham.Indicator WHERE IndicatorCode = 'G05'
GO

/*
====================================================
 Fisher Indicators (Fisher F01～F10 初始化)
====================================================
*/

INSERT INTO Fisher.Indicator
(
    IndicatorCode,
    IndicatorName,
    Category,
    Weight,
    MaxScore,
    Description,
    IsQuantitative
)
VALUES
(
    'F01',
    N'盈利稳定性',
    'Quality',
    10,
    10,
    N'过去多年盈利稳定程度。',
    1
),
(
    'F02',
    N'ROE水平',
    'Quality',
    10,
    10,
    N'长期资本回报水平。',
    1
),
(
    'F03',
    N'现金流质量',
    'Quality',
    10,
    10,
    N'利润与经营现金流匹配程度。',
    1
),
(
    'F04',
    N'毛利率',
    'Quality',
    10,
    10,
    N'长期盈利模式质量。',
    1
),
(
    'F05',
    N'净利率',
    'Quality',
    5,
    10,
    N'长期净利润率。',
    1
),
(
    'F06',
    N'营收成长',
    'Growth',
    15,
    10,
    N'长期收入增长能力。',
    1
),
(
    'F07',
    N'负债水平',
    'Risk',
    10,
    10,
    N'增长过程中财务杠杆的合理程度。',
    1
),
(
    'F08',
    N'分红能力',
    'ShareholderReturn',
    5,
    10,
    N'长期股东回报能力。',
    1
),
(
    'F09',
    N'行业地位',
    'CompetitiveAdvantage',
    10,
    10,
    N'行业排名、市场份额和竞争地位。',
    0
),
(
    'F10',
    N'护城河与长期竞争力',
    'CompetitiveAdvantage',
    15,
    10,
    N'品牌、成本、规模、网络、技术、渠道等长期竞争优势。',
    0
);
GO
INSERT INTO Fisher.ScoreRule
(
    IndicatorId,
    MinValue,
    MaxValue,
    Score,
    RuleOrder,
    Description,
    IsActive,
    CreatedTime
)
SELECT IndicatorId, 20, NULL, 10, 1, N'5年收入CAGR >= 20%', 1, SYSUTCDATETIME()
FROM Fisher.Indicator WHERE IndicatorCode = 'F06'
UNION ALL
SELECT IndicatorId, 15, 20, 8, 2, N'15% <= 5年收入CAGR < 20%', 1, SYSUTCDATETIME()
FROM Fisher.Indicator WHERE IndicatorCode = 'F06'
UNION ALL
SELECT IndicatorId, 10, 15, 6, 3, N'10% <= 5年收入CAGR < 15%', 1, SYSUTCDATETIME()
FROM Fisher.Indicator WHERE IndicatorCode = 'F06'
UNION ALL
SELECT IndicatorId, 5, 10, 3, 4, N'5% <= 5年收入CAGR < 10%', 1, SYSUTCDATETIME()
FROM Fisher.Indicator WHERE IndicatorCode = 'F06'
UNION ALL
SELECT IndicatorId, NULL, 5, 0, 5, N'5年收入CAGR < 5%', 1, SYSUTCDATETIME()
FROM Fisher.Indicator WHERE IndicatorCode = 'F06'
GO
/*
====================================================
 Quant.ScoreModel
====================================================
*/

INSERT INTO Quant.ScoreModel
(
    ModelCode,
    ModelName,
    Version,
    Description,
    IsActive
)
VALUES
(
    'VALUE_INVESTMENT',
    N'Buffett + Graham + Fisher 价值投资模型',
    'V2.0',
    N'以企业质量、安全边际、成长性、估值和行业地位为核心的综合价值投资模型。',
    1
);
GO
DECLARE @ModelId INT;

SELECT @ModelId = ModelId
FROM Quant.ScoreModel
WHERE ModelCode = 'VALUE_INVESTMENT'
  AND Version = 'V2.0';
INSERT INTO Quant.ScoreModelWeight
(
    ModelId,
    ComponentCode,
    Weight,
    MaxScore,
    Description
)
VALUES
(
    @ModelId,
    'BUFFETT',
    0.35,
    100,
    N'企业质量'
),
(
    @ModelId,
    'GRAHAM',
    0.25,
    100,
    N'安全边际'
),
(
    @ModelId,
    'FISHER',
    0.20,
    100,
    N'成长能力'
),
(
    @ModelId,
    'VALUATION',
    0.15,
    100,
    N'综合估值'
),
(
    @ModelId,
    'INDUSTRY',
    0.05,
    100,
    N'行业竞争位置'
);
GO

/*
====================================================
 Quant.ModelIndustryOverride
 行业模型覆盖
====================================================
*/

USE AStockQuant;
GO

CREATE TABLE Quant.ModelIndustryOverride
(
    Id BIGINT IDENTITY(1,1)
        CONSTRAINT PK_Quant_ModelIndustryOverride PRIMARY KEY,

    ModelId INT NOT NULL,

    IndustryId INT NOT NULL,

    ComponentCode VARCHAR(30) NOT NULL,

    Weight DECIMAL(10,6) NULL,

    RuleProfileCode VARCHAR(50) NULL,

    Enabled BIT NOT NULL
        CONSTRAINT DF_Quant_ModelIndustryOverride_Enabled
        DEFAULT (1),

    Description NVARCHAR(1000) NULL,

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Quant_ModelIndustryOverride_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Quant_ModelIndustryOverride_Model
        FOREIGN KEY (ModelId)
        REFERENCES Quant.ScoreModel(ModelId),

    CONSTRAINT FK_Quant_ModelIndustryOverride_Industry
        FOREIGN KEY (IndustryId)
        REFERENCES Basic.Industry(IndustryId),

    CONSTRAINT UQ_Quant_ModelIndustryOverride
        UNIQUE
        (
            ModelId,
            IndustryId,
            ComponentCode
        ),

    CONSTRAINT CK_Quant_ModelIndustryOverride_Weight
        CHECK
        (
            Weight IS NULL
            OR Weight >= 0
        )
);
GO
/*
====================================================
 Quant.RuleProfile
 规则配置档案
====================================================
*/

CREATE TABLE Quant.RuleProfile
(
    ProfileId INT IDENTITY(1,1)
        CONSTRAINT PK_Quant_RuleProfile PRIMARY KEY,

    ProfileCode VARCHAR(50) NOT NULL,

    ProfileName NVARCHAR(100) NOT NULL,

    Description NVARCHAR(1000) NULL,

    ModelVersion VARCHAR(30) NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Quant_RuleProfile_IsActive
        DEFAULT (1),

    CreatedTime DATETIME2(0) NOT NULL
        CONSTRAINT DF_Quant_RuleProfile_CreatedTime
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Quant_RuleProfile_Code
        UNIQUE (ProfileCode)
);
GO

ALTER TABLE Buffett.ScoreRule
ADD RuleProfileId INT NULL;
GO

ALTER TABLE Graham.ScoreRule
ADD RuleProfileId INT NULL;
GO

ALTER TABLE Fisher.ScoreRule
ADD RuleProfileId INT NULL;
GO

ALTER TABLE Buffett.ScoreRule
ADD CONSTRAINT FK_Buffett_ScoreRule_Profile
FOREIGN KEY (RuleProfileId)
REFERENCES Quant.RuleProfile(ProfileId);
GO

ALTER TABLE Graham.ScoreRule
ADD CONSTRAINT FK_Graham_ScoreRule_Profile
FOREIGN KEY (RuleProfileId)
REFERENCES Quant.RuleProfile(ProfileId);
GO

ALTER TABLE Fisher.ScoreRule
ADD CONSTRAINT FK_Fisher_ScoreRule_Profile
FOREIGN KEY (RuleProfileId)
REFERENCES Quant.RuleProfile(ProfileId);
GO

-----初始化规则档案
INSERT INTO Quant.RuleProfile
(
    ProfileCode,
    ProfileName,
    Description,
    ModelVersion
)
VALUES
(
    'DEFAULT',
    N'通用企业',
    N'默认A股企业评价规则。',
    'V2.0'
),
(
    'BANK',
    N'银行',
    N'银行业专用规则。',
    'V2.0'
),
(
    'INSURANCE',
    N'保险',
    N'保险业专用规则。',
    'V2.0'
),
(
    'BROKERAGE',
    N'券商',
    N'证券行业专用规则。',
    'V2.0'
),
(
    'CYCLICAL',
    N'周期行业',
    N'资源、能源、有色等周期行业规则。',
    'V2.0'
),
(
    'UTILITY',
    N'公用事业',
    N'电力、水务、燃气等行业规则。',
    'V2.0'
),
(
    'CONSUMER',
    N'消费',
    N'消费行业规则。',
    'V2.0'
),
(
    'TECHNOLOGY',
    N'科技',
    N'科技成长企业规则。',
    'V2.0'
),
(
    'MANUFACTURING',
    N'制造业',
    N'制造业规则。',
    'V2.0'
);
GO
