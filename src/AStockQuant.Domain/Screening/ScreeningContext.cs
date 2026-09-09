namespace AStockQuant.Domain.Screening;

public sealed record ScreeningContext(
		int StockId,
		string StockCode,
		string StockName,
		string? IndustryCode,
		string? IndustryName,
		DateOnly ScoreDate,
		DateOnly? ListingDate,
		DateOnly? FinancialReportDate,
		decimal BuffettScore,
		decimal GrahamScore,
		decimal FisherScore,
		decimal FinalScore,
		bool IsActive,
		bool IsST,
		bool HasCompleteFinancialData);