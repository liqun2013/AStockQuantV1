using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Domain.Services;

public sealed class CompositeScoreCalculator
{
    private readonly BuffettScoreCalculator buffett = new();
    private readonly GrahamScoreCalculator graham = new();
    private readonly FisherScoreCalculator fisher = new();
    public InvestmentScore Calculate(FinancialSnapshot snapshot, CompositeScoreWeights weights)
    {
        var buffettScore = buffett.Calculate(snapshot);
        var grahamScore = graham.Calculate(snapshot);
        var fisherScore = fisher.Calculate(snapshot);

        var finalScore = Math.Round(
            buffettScore.Score * weights.Buffett + grahamScore.Score * weights.Graham + fisherScore.Score * weights.Fisher,
            4,
            MidpointRounding.AwayFromZero);

        return new InvestmentScore(snapshot.StockCode, snapshot.AsOfDate, buffettScore, grahamScore, fisherScore, finalScore);
    }
}