namespace AStockQuant.Application.Screening;

public sealed record ScreeningRule(string RuleCode, decimal? NumericValue, bool? BoolValue, string? StringValue);

public sealed record ScreeningProfile(int ProfileId, string ProfileCode, string ProfileName, string Version, int ScoreModelId, bool IsActive, IReadOnlyList<ScreeningRule> Rules);

public sealed record ScreeningResultEntry(int StockId, string StockCode, decimal FinalScore, int? RankNo);

public sealed record ScreeningResult(long ResultId, int ProfileId, int ScoreModelId, DateTime RunDate, DateOnly? DataAsOfDate, IReadOnlyList<ScreeningResultEntry> Items);
