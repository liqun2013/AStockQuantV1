namespace AStockQuant.Domain.Screening;

public sealed record ScreeningDecision(ScreeningContext Context, bool IsSelected, string? RejectionReason);