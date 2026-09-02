namespace AStockQuant.Domain.ValueObjects;

public sealed record InvestmentScore(string StockCode, DateOnly ScoreDate, decimal BuffettScore, decimal GrahamScore, decimal FisherScore)
{
    public decimal FinalScore => Math.Round(BuffettScore * 0.40m + GrahamScore * 0.20m + FisherScore * 0.40m, 4, MidpointRounding.AwayFromZero);
}