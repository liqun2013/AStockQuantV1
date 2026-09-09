USE AStockQuant;
GO

IF OBJECT_ID('Quant.InvestmentScoreComponent', 'U') IS NULL
BEGIN
		CREATE TABLE Quant.InvestmentScoreComponent
		(
				ScoreComponentId BIGINT IDENTITY(1,1)
						CONSTRAINT PK_Quant_InvestmentScoreComponent PRIMARY KEY,
				ScoreId BIGINT NOT NULL,
				ModelCode VARCHAR(30) NOT NULL,
				ModelScore DECIMAL(10,4) NOT NULL,
				ComponentCode VARCHAR(60) NOT NULL,
				ComponentScore DECIMAL(10,4) NOT NULL,
				CreatedTime DATETIME2(0) NOT NULL
						CONSTRAINT DF_Quant_InvestmentScoreComponent_CreatedTime DEFAULT SYSUTCDATETIME(),
				CONSTRAINT FK_Quant_InvestmentScoreComponent_Score
						FOREIGN KEY (ScoreId) REFERENCES Quant.InvestmentScore(ScoreId),
				CONSTRAINT UQ_Quant_InvestmentScoreComponent
						UNIQUE (ScoreId, ModelCode, ComponentCode)
		);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Quant_InvestmentScoreComponent_Score' AND object_id = OBJECT_ID('Quant.InvestmentScoreComponent'))
BEGIN
		CREATE INDEX IX_Quant_InvestmentScoreComponent_Score
				ON Quant.InvestmentScoreComponent(ScoreId, ModelCode, ComponentCode);
END
GO
