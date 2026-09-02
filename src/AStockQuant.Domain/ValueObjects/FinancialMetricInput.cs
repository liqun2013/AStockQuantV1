namespace AStockQuant.Domain.ValueObjects;

public sealed record FinancialMetricInput(
		decimal? Revenue,
		decimal? PreviousRevenue,
		decimal? NetProfit,
		decimal? PreviousNetProfit,
		decimal? AverageEquity,
		decimal? InvestedCapital,
		decimal? GrossProfit,
		decimal? TotalAssets,
		decimal? TotalLiabilities,
		decimal? CurrentAssets,
		decimal? CurrentLiabilities,
		decimal? OperatingCashFlow,
		decimal? CapitalExpenditure);
