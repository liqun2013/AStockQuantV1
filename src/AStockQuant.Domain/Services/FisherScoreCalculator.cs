using AStockQuant.Domain.Interfaces;
using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Domain.Services;

public sealed class FisherScoreCalculator : IScoreCalculator
{
    public ScoreResult Calculate(FinancialSnapshot s)
    {
        var components = new Dictionary<string, decimal>
        {
            ["RevenueGrowth3Y"] = ScoreMath.Scale(s.RevenueGrowth3Y, 20m),
            ["ProfitGrowth3Y"] = ScoreMath.Scale(s.ProfitGrowth3Y, 20m),
            ["GrossMarginStability"] = ScoreMath.Scale(s.GrossMargin, 40m),
            ["Roic"] = ScoreMath.Scale(s.Roic, 15m),
            ["ResearchIntensity"] = ScoreMath.Scale(s.ResearchExpenseRatio, 8m)
        };
        var score = Math.Round(components.Values.Average(), 4, MidpointRounding.AwayFromZero);
        return new ScoreResult(score, ScoreMath.Grade(score), components);
    }
}