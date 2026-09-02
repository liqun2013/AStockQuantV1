namespace AStockQuant.Domain.ValueObjects;

public sealed record InvestmentIndicatorRule(
		string IndicatorCode,
		decimal Weight,
		decimal MaxScore,
		decimal? MinValue,
		decimal? MaxValue,
		decimal Score,
		int RuleOrder);
