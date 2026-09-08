namespace AStockQuant.Application.DTOs;

public sealed record StockScreeningRequest(
		DateOnly? ScoreDate = null,
		int TopN = 50,
		int MinimumListingYears = 3,
		decimal MinimumFisherScore = 60m,
		decimal MinimumBuffettScore = 60m,
		decimal MinimumGrahamScore = 50m,
		bool RequireCompleteFinancialData = true);

public sealed record StockCandidateDto(
		string StockCode,
		string StockName,
		string? IndustryCode,
		string? IndustryName,
		DateOnly ScoreDate,
		DateOnly? FinancialReportDate,
		decimal BuffettScore,
		decimal GrahamScore,
		decimal FisherScore,
		decimal FinalScore);
