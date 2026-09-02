using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Domain.Interfaces;

public interface IScoreCalculator
{
    ScoreResult Calculate(FinancialSnapshot snapshot);
}