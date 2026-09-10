namespace AStockQuant.Domain.Screening;

public sealed record IndustryScreeningRuleOverride(string IndustryCode, string RuleCode, decimal NumericValue);