namespace AStockQuant.Domain.ValueObjects;

public sealed record InvestmentScore(
    string StockCode,
    DateOnly ScoreDate,
    ScoreResult Buffett,
    ScoreResult Graham,
    ScoreResult Fisher,
    decimal FinalScore)
{
    public decimal BuffettScore => Buffett.Score;
    public decimal GrahamScore => Graham.Score;
    public decimal FisherScore => Fisher.Score;
}
