USE AStockQuant;
GO

IF COL_LENGTH('Basic.Stock', 'IsST') IS NULL
BEGIN
		ALTER TABLE Basic.Stock
				ADD IsST BIT NOT NULL
						CONSTRAINT DF_Basic_Stock_IsST DEFAULT (0);
END
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Strategy_ScreeningRule_Code' AND parent_object_id = OBJECT_ID('Strategy.ScreeningRule'))
BEGIN
		ALTER TABLE Strategy.ScreeningRule DROP CONSTRAINT CK_Strategy_ScreeningRule_Code;
END
GO

ALTER TABLE Strategy.ScreeningRule
		ADD CONSTRAINT CK_Strategy_ScreeningRule_Code
		CHECK (RuleCode IN ('TOP_N','MINIMUM_LISTING_YEARS','MINIMUM_FISHER_SCORE','MINIMUM_BUFFETT_SCORE','MINIMUM_GRAHAM_SCORE','MINIMUM_FINAL_SCORE','REQUIRE_COMPLETE_FINANCIAL_DATA','EXCLUDE_ST'));
GO

DECLARE @ProfileId INT;
SELECT @ProfileId = ProfileId
FROM Strategy.ScreeningProfile
WHERE ProfileCode = 'VALUE_QUALITY'
	AND Version = 'V1.0';

IF @ProfileId IS NOT NULL
	 AND NOT EXISTS (SELECT 1 FROM Strategy.ScreeningRule WHERE ProfileId = @ProfileId AND RuleCode = 'EXCLUDE_ST')
BEGIN
		INSERT INTO Strategy.ScreeningRule (ProfileId, RuleCode, BoolValue)
		VALUES (@ProfileId, 'EXCLUDE_ST', 1);
END

IF @ProfileId IS NOT NULL
	 AND NOT EXISTS (SELECT 1 FROM Strategy.ScreeningRule WHERE ProfileId = @ProfileId AND RuleCode = 'MINIMUM_FINAL_SCORE')
BEGIN
		INSERT INTO Strategy.ScreeningRule (ProfileId, RuleCode, NumericValue)
		VALUES (@ProfileId, 'MINIMUM_FINAL_SCORE', 0);
END
GO
