using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Domain.Services;

public sealed class CompositeScoreCalculator
{
    public InvestmentScore Calculate(string stockCode, DateOnly asOfDate, ScoreResult buffettScore, ScoreResult grahamScore, ScoreResult fisherScore, CompositeScoreWeights weights)
    {

        var finalScore = Math.Round(
            buffettScore.Score * weights.Buffett + grahamScore.Score * weights.Graham + fisherScore.Score * weights.Fisher,
            4,
            MidpointRounding.AwayFromZero);

        return new InvestmentScore(stockCode, asOfDate, buffettScore, grahamScore, fisherScore, finalScore);
    }
}