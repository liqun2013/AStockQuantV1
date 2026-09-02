using AStockQuant.Domain.Interfaces;
using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Domain.Services;

public sealed class BuffettScoreCalculator : IScoreCalculator
{
    public ScoreResult Calculate(FinancialSnapshot s)
    {
        var components = new Dictionary<string, decimal>
        {
            ["Roe"] = ScoreMath.Scale(s.Roe, 20m),
            ["Roic"] = ScoreMath.Scale(s.Roic, 15m),
            ["GrossMargin"] = ScoreMath.Scale(s.GrossMargin, 45m),
            ["CashConversion"] = ScoreMath.Scale(s.OperatingCashFlowToNetProfit, 1m),
            ["DebtSafety"] = ScoreMath.Clamp((1m - s.DebtAssetRatio / 70m) * 100m),
            ["Liquidity"] = ScoreMath.Scale(s.CurrentRatio, 2m)
        };
        var score = Math.Round(components.Values.Average(), 4, MidpointRounding.AwayFromZero);
        return new ScoreResult(score, ScoreMath.Grade(score), components);
    }
}