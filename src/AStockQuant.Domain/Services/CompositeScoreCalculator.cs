using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Domain.Services;

public sealed class CompositeScoreCalculator
{
    private readonly BuffettScoreCalculator buffett = new();
    private readonly GrahamScoreCalculator graham = new();
    private readonly FisherScoreCalculator fisher = new();
    public InvestmentScore Calculate(FinancialSnapshot snapshot)
    {
        return new InvestmentScore(snapshot.StockCode, snapshot.AsOfDate, buffett.Calculate(snapshot).Score, graham.Calculate(snapshot).Score, fisher.Calculate(snapshot).Score);
    }
}