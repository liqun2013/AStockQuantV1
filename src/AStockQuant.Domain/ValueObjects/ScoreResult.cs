namespace AStockQuant.Domain.ValueObjects;

public sealed record ScoreResult(decimal Score, string Grade, IReadOnlyDictionary<string, decimal> Components);