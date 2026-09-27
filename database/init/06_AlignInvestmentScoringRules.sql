USE AStockQuant;
GO

UPDATE Buffett.Indicator
SET IndicatorName = CASE IndicatorCode
				WHEN 'B01' THEN N'长期 ROE'
				WHEN 'B02' THEN N'自由现金流利润率'
				WHEN 'B03' THEN N'利润稳定性'
				WHEN 'B04' THEN N'负债水平'
				WHEN 'B05' THEN N'竞争优势'
		END,
		Category = CASE IndicatorCode
				WHEN 'B01' THEN 'Profitability'
				WHEN 'B02' THEN 'CashFlow'
				WHEN 'B03' THEN 'Stability'
				WHEN 'B04' THEN 'FinancialSafety'
				WHEN 'B05' THEN 'CompetitiveAdvantage'
		END,
		Description = CASE IndicatorCode
				WHEN 'B01' THEN N'过去五个完整年度的平均 ROE。'
				WHEN 'B02' THEN N'自由现金流占收入的比例。'
				WHEN 'B03' THEN N'过去五个完整年度盈利为正的比例。'
				WHEN 'B04' THEN N'资产负债率。'
				WHEN 'B05' THEN N'品牌、成本、规模、网络、技术和渠道等竞争优势，需人工评价。'
		END,
		Weight = CASE IndicatorCode WHEN 'B01' THEN 25 WHEN 'B02' THEN 25 WHEN 'B03' THEN 20 WHEN 'B04' THEN 15 WHEN 'B05' THEN 15 END,
		IsQuantitative = CASE WHEN IndicatorCode = 'B05' THEN 0 ELSE 1 END,
		IsActive = 1
WHERE IndicatorCode IN ('B01', 'B02', 'B03', 'B04', 'B05');
UPDATE Buffett.Indicator SET IsActive = 0 WHERE IndicatorCode IN ('B06', 'B07', 'B08');

UPDATE Graham.Indicator
SET IndicatorName = CASE IndicatorCode
				WHEN 'G01' THEN N'PE 估值'
				WHEN 'G02' THEN N'PB 估值'
				WHEN 'G03' THEN N'股息率'
				WHEN 'G04' THEN N'资产安全性'
				WHEN 'G05' THEN N'盈利稳定性'
		END,
		Category = CASE IndicatorCode
				WHEN 'G01' THEN 'Valuation'
				WHEN 'G02' THEN 'Valuation'
				WHEN 'G03' THEN 'ShareholderReturn'
				WHEN 'G04' THEN 'FinancialSafety'
				WHEN 'G05' THEN 'EarningsStability'
		END,
		Description = CASE IndicatorCode
				WHEN 'G01' THEN N'市盈率估值水平。'
				WHEN 'G02' THEN N'市净率估值水平。'
				WHEN 'G03' THEN N'当前股息率。'
				WHEN 'G04' THEN N'资产负债率衡量的资产安全性。'
				WHEN 'G05' THEN N'过去五个完整年度盈利为正的比例。'
		END,
		Weight = CASE IndicatorCode WHEN 'G01' THEN 30 WHEN 'G02' THEN 20 WHEN 'G03' THEN 20 WHEN 'G04' THEN 15 WHEN 'G05' THEN 15 END,
		IsActive = 1
WHERE IndicatorCode IN ('G01', 'G02', 'G03', 'G04', 'G05');
UPDATE Graham.Indicator SET IsActive = 0 WHERE IndicatorCode IN ('G06', 'G07', 'G08');

UPDATE Fisher.Indicator
SET IndicatorName = CASE IndicatorCode
				WHEN 'F01' THEN N'收入增长'
				WHEN 'F02' THEN N'利润增长'
				WHEN 'F03' THEN N'ROE'
				WHEN 'F04' THEN N'行业成长空间'
				WHEN 'F05' THEN N'竞争优势'
		END,
		Category = CASE IndicatorCode
				WHEN 'F01' THEN 'Growth'
				WHEN 'F02' THEN 'Growth'
				WHEN 'F03' THEN 'Profitability'
				WHEN 'F04' THEN 'IndustryGrowth'
				WHEN 'F05' THEN 'CompetitiveAdvantage'
		END,
		Description = CASE IndicatorCode
				WHEN 'F01' THEN N'过去三个完整年度的收入复合增长率。'
				WHEN 'F02' THEN N'过去三个完整年度的净利润复合增长率。'
				WHEN 'F03' THEN N'过去五个完整年度的平均 ROE。'
				WHEN 'F04' THEN N'行业长期成长空间，当前数据库尚无量化数据。'
				WHEN 'F05' THEN N'品牌、成本、规模、网络、技术和渠道等竞争优势，需人工评价。'
		END,
		Weight = CASE IndicatorCode WHEN 'F01' THEN 20 WHEN 'F02' THEN 25 WHEN 'F03' THEN 25 WHEN 'F04' THEN 15 WHEN 'F05' THEN 15 END,
		IsQuantitative = CASE WHEN IndicatorCode IN ('F01', 'F02', 'F03') THEN 1 ELSE 0 END,
		IsActive = 1
WHERE IndicatorCode IN ('F01', 'F02', 'F03', 'F04', 'F05');
UPDATE Fisher.Indicator SET IsActive = 0 WHERE IndicatorCode IN ('F06', 'F07', 'F08', 'F09', 'F10');
GO

DECLARE @DefaultProfileId INT = (SELECT ProfileId FROM Quant.RuleProfile WHERE ProfileCode = 'DEFAULT' AND IsActive = 1);

DELETE FROM Buffett.ScoreRule WHERE RuleProfileId IS NULL OR RuleProfileId = @DefaultProfileId;
DELETE FROM Graham.ScoreRule WHERE RuleProfileId IS NULL OR RuleProfileId = @DefaultProfileId;
DELETE FROM Fisher.ScoreRule WHERE RuleProfileId IS NULL OR RuleProfileId = @DefaultProfileId;

CREATE TABLE #RuleSeed
(
		ModelCode VARCHAR(10) NOT NULL,
		IndicatorCode VARCHAR(20) NOT NULL,
		MinValue DECIMAL(24,8) NULL,
		MaxValue DECIMAL(24,8) NULL,
		Score DECIMAL(10,4) NOT NULL,
		RuleOrder INT NOT NULL,
		Description NVARCHAR(500) NOT NULL
);

INSERT INTO #RuleSeed (ModelCode, IndicatorCode, MinValue, MaxValue, Score, RuleOrder, Description)
VALUES
('BUFFETT', 'B01', 25, NULL, 10, 1, N'ROE >= 25%'),
('BUFFETT', 'B01', 20, 25, 9, 2, N'20% <= ROE < 25%'),
('BUFFETT', 'B01', 15, 20, 7, 3, N'15% <= ROE < 20%'),
('BUFFETT', 'B01', 10, 15, 5, 4, N'10% <= ROE < 15%'),
('BUFFETT', 'B01', 5, 10, 2, 5, N'5% <= ROE < 10%'),
('BUFFETT', 'B01', NULL, 5, 0, 6, N'ROE < 5%'),
('BUFFETT', 'B02', 20, NULL, 10, 1, N'FCF Margin >= 20%'),
('BUFFETT', 'B02', 15, 20, 8, 2, N'15% <= FCF Margin < 20%'),
('BUFFETT', 'B02', 10, 15, 6, 3, N'10% <= FCF Margin < 15%'),
('BUFFETT', 'B02', 5, 10, 3, 4, N'5% <= FCF Margin < 10%'),
('BUFFETT', 'B02', NULL, 5, 0, 5, N'FCF Margin < 5%'),
('BUFFETT', 'B03', 1, NULL, 10, 1, N'5 年盈利均为正'),
('BUFFETT', 'B03', 0.8, 1, 8, 2, N'5 年盈利稳定性 >= 80%'),
('BUFFETT', 'B03', 0.6, 0.8, 6, 3, N'5 年盈利稳定性 >= 60%'),
('BUFFETT', 'B03', 0.4, 0.6, 3, 4, N'5 年盈利稳定性 >= 40%'),
('BUFFETT', 'B03', NULL, 0.4, 0, 5, N'5 年盈利稳定性 < 40%'),
('BUFFETT', 'B04', NULL, 30, 10, 1, N'资产负债率 < 30%'),
('BUFFETT', 'B04', 30, 50, 8, 2, N'30% <= 资产负债率 < 50%'),
('BUFFETT', 'B04', 50, 60, 6, 3, N'50% <= 资产负债率 < 60%'),
('BUFFETT', 'B04', 60, 70, 3, 4, N'60% <= 资产负债率 < 70%'),
('BUFFETT', 'B04', 70, NULL, 0, 5, N'资产负债率 >= 70%'),
('GRAHAM', 'G01', NULL, 10, 10, 1, N'PE < 10'),
('GRAHAM', 'G01', 10, 15, 8, 2, N'10 <= PE < 15'),
('GRAHAM', 'G01', 15, 20, 6, 3, N'15 <= PE < 20'),
('GRAHAM', 'G01', 20, 30, 3, 4, N'20 <= PE < 30'),
('GRAHAM', 'G01', 30, NULL, 0, 5, N'PE >= 30'),
('GRAHAM', 'G02', NULL, 1.5, 10, 1, N'PB < 1.5'),
('GRAHAM', 'G02', 1.5, 2, 8, 2, N'1.5 <= PB < 2'),
('GRAHAM', 'G02', 2, 3, 6, 3, N'2 <= PB < 3'),
('GRAHAM', 'G02', 3, 5, 3, 4, N'3 <= PB < 5'),
('GRAHAM', 'G02', 5, NULL, 0, 5, N'PB >= 5'),
('GRAHAM', 'G03', 5, NULL, 10, 1, N'股息率 >= 5%'),
('GRAHAM', 'G03', 4, 5, 8, 2, N'4% <= 股息率 < 5%'),
('GRAHAM', 'G03', 3, 4, 6, 3, N'3% <= 股息率 < 4%'),
('GRAHAM', 'G03', 2, 3, 3, 4, N'2% <= 股息率 < 3%'),
('GRAHAM', 'G03', NULL, 2, 0, 5, N'股息率 < 2%'),
('GRAHAM', 'G04', NULL, 30, 10, 1, N'资产负债率 < 30%'),
('GRAHAM', 'G04', 30, 50, 8, 2, N'30% <= 资产负债率 < 50%'),
('GRAHAM', 'G04', 50, 60, 6, 3, N'50% <= 资产负债率 < 60%'),
('GRAHAM', 'G04', 60, 70, 3, 4, N'60% <= 资产负债率 < 70%'),
('GRAHAM', 'G04', 70, NULL, 0, 5, N'资产负债率 >= 70%'),
('GRAHAM', 'G05', 1, NULL, 10, 1, N'5 年盈利均为正'),
('GRAHAM', 'G05', 0.8, 1, 8, 2, N'5 年盈利稳定性 >= 80%'),
('GRAHAM', 'G05', 0.6, 0.8, 6, 3, N'5 年盈利稳定性 >= 60%'),
('GRAHAM', 'G05', 0.4, 0.6, 3, 4, N'5 年盈利稳定性 >= 40%'),
('GRAHAM', 'G05', NULL, 0.4, 0, 5, N'5 年盈利稳定性 < 40%'),
('FISHER', 'F01', 30, NULL, 10, 1, N'3 年收入 CAGR >= 30%'),
('FISHER', 'F01', 20, 30, 8, 2, N'20% <= 3 年收入 CAGR < 30%'),
('FISHER', 'F01', 10, 20, 6, 3, N'10% <= 3 年收入 CAGR < 20%'),
('FISHER', 'F01', NULL, 10, 2, 4, N'3 年收入 CAGR < 10%'),
('FISHER', 'F02', 30, NULL, 10, 1, N'3 年利润 CAGR >= 30%'),
('FISHER', 'F02', 20, 30, 8, 2, N'20% <= 3 年利润 CAGR < 30%'),
('FISHER', 'F02', 10, 20, 6, 3, N'10% <= 3 年利润 CAGR < 20%'),
('FISHER', 'F02', NULL, 10, 2, 4, N'3 年利润 CAGR < 10%'),
('FISHER', 'F03', 25, NULL, 10, 1, N'ROE >= 25%'),
('FISHER', 'F03', 15, 25, 8, 2, N'15% <= ROE < 25%'),
('FISHER', 'F03', 10, 15, 5, 3, N'10% <= ROE < 15%'),
('FISHER', 'F03', NULL, 10, 2, 4, N'ROE < 10%');

INSERT INTO Buffett.ScoreRule (IndicatorId, MinValue, MaxValue, Score, RuleOrder, Description, IsActive)
SELECT indicator.IndicatorId, seed.MinValue, seed.MaxValue, seed.Score, seed.RuleOrder, seed.Description, 1
FROM #RuleSeed seed INNER JOIN Buffett.Indicator indicator ON indicator.IndicatorCode = seed.IndicatorCode
WHERE seed.ModelCode = 'BUFFETT';

INSERT INTO Graham.ScoreRule (IndicatorId, MinValue, MaxValue, Score, RuleOrder, Description, IsActive)
SELECT indicator.IndicatorId, seed.MinValue, seed.MaxValue, seed.Score, seed.RuleOrder, seed.Description, 1
FROM #RuleSeed seed INNER JOIN Graham.Indicator indicator ON indicator.IndicatorCode = seed.IndicatorCode
WHERE seed.ModelCode = 'GRAHAM';

INSERT INTO Fisher.ScoreRule (IndicatorId, MinValue, MaxValue, Score, RuleOrder, Description, IsActive)
SELECT indicator.IndicatorId, seed.MinValue, seed.MaxValue, seed.Score, seed.RuleOrder, seed.Description, 1
FROM #RuleSeed seed INNER JOIN Fisher.Indicator indicator ON indicator.IndicatorCode = seed.IndicatorCode
WHERE seed.ModelCode = 'FISHER';
GO
