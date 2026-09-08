namespace AStockQuant.Domain.ValueObjects;

public sealed record InvestmentScore(
    string StockCode,
    DateOnly ScoreDate,
    decimal BuffettScore,
    decimal GrahamScore,
    decimal FisherScore,
    decimal FinalScore);
