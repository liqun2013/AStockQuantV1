using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Domain.Services;

public sealed class CompositeScoreCalculator
{
    private readonly BuffettScoreCalculator buffett = new();
    private readonly GrahamScoreCalculator graham = new();
    private readonly FisherScoreCalculator fisher = new();
    public InvestmentScore Calculate(FinancialSnapshot snapshot, CompositeScoreWeights weights)
    {
        var buffettScore = buffett.Calculate(snapshot).Score;
        var grahamScore = graham.Calculate(snapshot).Score;
        var fisherScore = fisher.Calculate(snapshot).Score;

        var finalScore = Math.Round(
            buffettScore * weights.Buffett + grahamScore * weights.Graham + fisherScore * weights.Fisher,
            4,
            MidpointRounding.AwayFromZero);

        return new InvestmentScore(snapshot.StockCode, snapshot.AsOfDate, buffettScore, grahamScore, fisherScore, finalScore);
    }
}