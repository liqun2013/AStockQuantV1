namespace AStockQuant.Domain.ValueObjects;

public sealed record FinancialMetrics(
		decimal? Roe,
		decimal? Roic,
		decimal? GrossMargin,
		decimal? NetMargin,
		decimal? DebtAssetRatio,
		decimal? CurrentRatio,
		decimal? OperatingCashFlowToNetProfit,
		decimal? FreeCashFlow,
		decimal? FreeCashFlowMargin,
		decimal? RevenueGrowth,
		decimal? NetProfitGrowth);
