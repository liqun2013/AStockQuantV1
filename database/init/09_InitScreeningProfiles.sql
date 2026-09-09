USE AStockQuant;
GO

-- 初始化筛选档案：VALUE_QUALITY，关联 VALUE_INVESTMENT V2.0
DECLARE @ModelId INT;
SELECT @ModelId = ModelId FROM Quant.ScoreModel WHERE ModelCode = 'VALUE_INVESTMENT' AND Version = 'V2.0';

IF @ModelId IS NULL
BEGIN
		PRINT 'ERROR: 未找到 VALUE_INVESTMENT V2.0 的 ScoreModelId，请先运行 05_InsertInvestmentRules.sql';
END
ELSE
BEGIN
		IF NOT EXISTS (SELECT 1 FROM Strategy.ScreeningProfile WHERE ProfileCode = 'VALUE_QUALITY' AND Version = 'V1.0')
		BEGIN
				INSERT INTO Strategy.ScreeningProfile (ProfileCode, ProfileName, Description, Version, ScoreModelId, IsActive)
				VALUES ('VALUE_QUALITY', N'价值型-质量优先', N'价值投资，偏好企业质量与估值安全', 'V1.0', @ModelId, 1);
		END

		DECLARE @ProfileId INT;
		SELECT @ProfileId = ProfileId FROM Strategy.ScreeningProfile WHERE ProfileCode = 'VALUE_QUALITY' AND Version = 'V1.0';

		-- 插入规则（若不存在则插入）
		IF NOT EXISTS (SELECT 1 FROM Strategy.ScreeningRule WHERE ProfileId = @ProfileId AND RuleCode = 'TOP_N')
				INSERT INTO Strategy.ScreeningRule (ProfileId, RuleCode, NumericValue) VALUES (@ProfileId, 'TOP_N', 50);

		IF NOT EXISTS (SELECT 1 FROM Strategy.ScreeningRule WHERE ProfileId = @ProfileId AND RuleCode = 'MINIMUM_LISTING_YEARS')
				INSERT INTO Strategy.ScreeningRule (ProfileId, RuleCode, NumericValue) VALUES (@ProfileId, 'MINIMUM_LISTING_YEARS', 3);

		IF NOT EXISTS (SELECT 1 FROM Strategy.ScreeningRule WHERE ProfileId = @ProfileId AND RuleCode = 'MINIMUM_FISHER_SCORE')
				INSERT INTO Strategy.ScreeningRule (ProfileId, RuleCode, NumericValue) VALUES (@ProfileId, 'MINIMUM_FISHER_SCORE', 60);

		IF NOT EXISTS (SELECT 1 FROM Strategy.ScreeningRule WHERE ProfileId = @ProfileId AND RuleCode = 'MINIMUM_BUFFETT_SCORE')
				INSERT INTO Strategy.ScreeningRule (ProfileId, RuleCode, NumericValue) VALUES (@ProfileId, 'MINIMUM_BUFFETT_SCORE', 60);

		IF NOT EXISTS (SELECT 1 FROM Strategy.ScreeningRule WHERE ProfileId = @ProfileId AND RuleCode = 'MINIMUM_GRAHAM_SCORE')
				INSERT INTO Strategy.ScreeningRule (ProfileId, RuleCode, NumericValue) VALUES (@ProfileId, 'MINIMUM_GRAHAM_SCORE', 50);

		IF NOT EXISTS (SELECT 1 FROM Strategy.ScreeningRule WHERE ProfileId = @ProfileId AND RuleCode = 'REQUIRE_COMPLETE_FINANCIAL_DATA')
				INSERT INTO Strategy.ScreeningRule (ProfileId, RuleCode, BoolValue) VALUES (@ProfileId, 'REQUIRE_COMPLETE_FINANCIAL_DATA', 1);

		IF NOT EXISTS (SELECT 1 FROM Strategy.ScreeningRule WHERE ProfileId = @ProfileId AND RuleCode = 'EXCLUDE_ST')
				INSERT INTO Strategy.ScreeningRule (ProfileId, RuleCode, BoolValue) VALUES (@ProfileId, 'EXCLUDE_ST', 1);
END
GO
