namespace AStockQuant.Domain.Screening;

public sealed record ScreeningRule(string RuleCode, decimal? NumericValue, bool? BoolValue, string? StringValue);