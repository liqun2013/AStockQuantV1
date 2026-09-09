USE AStockQuant;
GO

/*
====================================================
Strategy.ScreeningProfile
候选筛选档案
====================================================
*/
CREATE TABLE Strategy.ScreeningProfile
(
		ProfileId INT IDENTITY(1,1) CONSTRAINT PK_Strategy_ScreeningProfile PRIMARY KEY,
		ProfileCode VARCHAR(50) NOT NULL,
		ProfileName NVARCHAR(200) NOT NULL,
		Description NVARCHAR(1000) NULL,
		Version VARCHAR(30) NOT NULL,
		ScoreModelId INT NOT NULL,
		IsActive BIT NOT NULL CONSTRAINT DF_Strategy_ScreeningProfile_IsActive DEFAULT (1),
		CreatedTime DATETIME2(0) NOT NULL CONSTRAINT DF_Strategy_ScreeningProfile_CreatedTime DEFAULT SYSUTCDATETIME(),
		CONSTRAINT FK_Strategy_ScreeningProfile_Model FOREIGN KEY (ScoreModelId) REFERENCES Quant.ScoreModel(ModelId),
		CONSTRAINT UQ_Strategy_ScreeningProfile_Code_Version UNIQUE (ProfileCode, Version)
);
GO

/*
====================================================
Strategy.ScreeningRule
保存筛选档案的规则项
RuleCode 采用预定义的枚举字符串
====================================================
*/
CREATE TABLE Strategy.ScreeningRule
(
		RuleId INT IDENTITY(1,1) CONSTRAINT PK_Strategy_ScreeningRule PRIMARY KEY,
		ProfileId INT NOT NULL,
		RuleCode VARCHAR(60) NOT NULL,
		NumericValue DECIMAL(28,8) NULL,
		BoolValue BIT NULL,
		StringValue NVARCHAR(500) NULL,
		CreatedTime DATETIME2(0) NOT NULL CONSTRAINT DF_Strategy_ScreeningRule_CreatedTime DEFAULT SYSUTCDATETIME(),
		CONSTRAINT FK_Strategy_ScreeningRule_Profile FOREIGN KEY (ProfileId) REFERENCES Strategy.ScreeningProfile(ProfileId),
		CONSTRAINT UQ_Strategy_ScreeningRule_Profile_Rule UNIQUE (ProfileId, RuleCode)
);
GO

-- 可选：限制 RuleCode 为已知值
ALTER TABLE Strategy.ScreeningRule ADD CONSTRAINT CK_Strategy_ScreeningRule_Code CHECK (RuleCode IN ('TOP_N','MINIMUM_LISTING_YEARS','MINIMUM_FISHER_SCORE','MINIMUM_BUFFETT_SCORE','MINIMUM_GRAHAM_SCORE','REQUIRE_COMPLETE_FINANCIAL_DATA'));
GO

/*
====================================================
Strategy.ScreeningResult & Items
记录一次筛选执行的结果用于追溯
====================================================
*/
CREATE TABLE Strategy.ScreeningResult
(
		ResultId BIGINT IDENTITY(1,1) CONSTRAINT PK_Strategy_ScreeningResult PRIMARY KEY,
		ProfileId INT NOT NULL,
		ScoreModelId INT NOT NULL,
		RunDate DATETIME2(0) NOT NULL CONSTRAINT DF_Strategy_ScreeningResult_RunDate DEFAULT SYSUTCDATETIME(),
		DataAsOfDate DATE NULL,
		CandidateCount INT NULL,
		CreatedTime DATETIME2(0) NOT NULL CONSTRAINT DF_Strategy_ScreeningResult_CreatedTime DEFAULT SYSUTCDATETIME(),
		CONSTRAINT FK_Strategy_ScreeningResult_Profile FOREIGN KEY (ProfileId) REFERENCES Strategy.ScreeningProfile(ProfileId),
		CONSTRAINT FK_Strategy_ScreeningResult_Model FOREIGN KEY (ScoreModelId) REFERENCES Quant.ScoreModel(ModelId)
);
GO

CREATE TABLE Strategy.ScreeningResultItem
(
		ResultItemId BIGINT IDENTITY(1,1) CONSTRAINT PK_Strategy_ScreeningResultItem PRIMARY KEY,
		ResultId BIGINT NOT NULL,
		StockId INT NOT NULL,
		RankNo INT NULL,
		FinalScore DECIMAL(18,6) NULL,
		DataAsOfDate DATE NULL,
		CreatedTime DATETIME2(0) NOT NULL CONSTRAINT DF_Strategy_ScreeningResultItem_CreatedTime DEFAULT SYSUTCDATETIME(),
		CONSTRAINT FK_Strategy_ScreeningResultItem_Result FOREIGN KEY (ResultId) REFERENCES Strategy.ScreeningResult(ResultId),
		CONSTRAINT FK_Strategy_ScreeningResultItem_Stock FOREIGN KEY (StockId) REFERENCES Basic.Stock(StockId)
);
GO

CREATE INDEX IX_Strategy_ScreeningResult_Profile_RunDate ON Strategy.ScreeningResult(ProfileId, RunDate DESC);
CREATE INDEX IX_Strategy_ScreeningResultItem_Result_Stock ON Strategy.ScreeningResultItem(ResultId, StockId);
GO
