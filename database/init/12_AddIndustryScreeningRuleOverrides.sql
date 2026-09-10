USE AStockQuant;
GO

IF OBJECT_ID('Strategy.IndustryScreeningRuleOverride', 'U') IS NULL
BEGIN
		CREATE TABLE Strategy.IndustryScreeningRuleOverride
		(
				IndustryScreeningRuleOverrideId BIGINT IDENTITY(1,1)
						CONSTRAINT PK_Strategy_IndustryScreeningRuleOverride PRIMARY KEY,
				ScoreModelId INT NOT NULL,
				IndustryId INT NOT NULL,
				RuleCode VARCHAR(60) NOT NULL,
				NumericValue DECIMAL(28,8) NOT NULL,
				IsActive BIT NOT NULL
						CONSTRAINT DF_Strategy_IndustryScreeningRuleOverride_IsActive DEFAULT (1),
				CreatedTime DATETIME2(0) NOT NULL
						CONSTRAINT DF_Strategy_IndustryScreeningRuleOverride_CreatedTime DEFAULT SYSUTCDATETIME(),
				CONSTRAINT FK_Strategy_IndustryScreeningRuleOverride_Model
						FOREIGN KEY (ScoreModelId) REFERENCES Quant.ScoreModel(ModelId),
				CONSTRAINT FK_Strategy_IndustryScreeningRuleOverride_Industry
						FOREIGN KEY (IndustryId) REFERENCES Basic.Industry(IndustryId),
				CONSTRAINT UQ_Strategy_IndustryScreeningRuleOverride
						UNIQUE (ScoreModelId, IndustryId, RuleCode),
				CONSTRAINT CK_Strategy_IndustryScreeningRuleOverride_Code
						CHECK (RuleCode IN ('MINIMUM_BUFFETT_SCORE', 'MINIMUM_GRAHAM_SCORE', 'MINIMUM_FISHER_SCORE', 'MINIMUM_FINAL_SCORE')),
				CONSTRAINT CK_Strategy_IndustryScreeningRuleOverride_Value
						CHECK (NumericValue BETWEEN 0 AND 100)
		);
END
GO
