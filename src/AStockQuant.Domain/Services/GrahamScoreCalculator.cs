using AStockQuant.Domain.Interfaces;
using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Domain.Services;

public sealed class GrahamScoreCalculator : IScoreCalculator
{
    public ScoreResult Calculate(FinancialSnapshot s)
    {
        var grahamNumber = s.Eps > 0m && s.Bvps > 0m ? (decimal)Math.Sqrt((double)(22.5m * s.Eps * s.Bvps)) : 0m;
        var marginOfSafety = grahamNumber > 0m ? (grahamNumber - s.MarketPrice) / grahamNumber : -1m;
        var components = new Dictionary<string, decimal>
        {
            ["Pe"] = s.Pe <= 0m ? 0m : ScoreMath.Clamp((30m - s.Pe) / 20m * 100m),
            ["Pb"] = s.Pb <= 0m ? 0m : ScoreMath.Clamp((3m - s.Pb) / 2m * 100m),
            ["GrahamNumber"] = s.MarketPrice > 0m ? ScoreMath.Clamp(grahamNumber / s.MarketPrice * 70m) : 0m,
            ["MarginOfSafety"] = ScoreMath.Clamp((marginOfSafety + 0.2m) / 0.5m * 100m),
            ["FinancialSafety"] = ScoreMath.Clamp((1m - s.DebtAssetRatio / 60m) * 100m)
        };
        var score = Math.Round(components.Values.Average(), 4, MidpointRounding.AwayFromZero);
        return new ScoreResult(score, ScoreMath.Grade(score), components);
    }
}