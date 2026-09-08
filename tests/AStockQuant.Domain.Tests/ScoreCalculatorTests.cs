using AStockQuant.Domain.Services;
using AStockQuant.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AStockQuant.Domain.Tests;

public sealed class ScoreCalculatorTests
{
    [Fact]
    public void BuffettCalculator_ReturnsExcellentScore_ForHighQualityCompany()
    {
        var result = new BuffettScoreCalculator().Calculate(CreateExcellentSnapshot());
        result.Score.Should().BeGreaterThan(85m);
        result.Grade.Should().Be("Excellent");
    }

    [Fact]
    public void CompositeCalculator_UsesDeterministicWeightedScore()
    {
        var weights = new CompositeScoreWeights(0.40m, 0.20m, 0.40m);
        var score = new CompositeScoreCalculator().Calculate(CreateExcellentSnapshot(), weights);
        score.FinalScore.Should().Be(Math.Round(score.BuffettScore * weights.Buffett + score.GrahamScore * weights.Graham + score.FisherScore * weights.Fisher, 4, MidpointRounding.AwayFromZero));
    }

    [Fact]
    public void CompositeScoreWeights_RejectsWeightsThatDoNotSumToOne()
    {
        var action = () => new CompositeScoreWeights(0.40m, 0.20m, 0.30m);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CompositeScoreWeights_RejectsNegativeWeights()
    {
        var action = () => new CompositeScoreWeights(-0.10m, 0.20m, 0.90m);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GrahamCalculator_RewardsPositiveMarginOfSafety()
    {
        var result = new GrahamScoreCalculator().Calculate(CreateExcellentSnapshot() with { MarketPrice = 10m, Eps = 2m, Bvps = 12m });
        result.Components["MarginOfSafety"].Should().BeGreaterThan(90m);
    }

    [Fact]
    public void FinancialMetricCalculator_CalculatesPercentMetrics()
    {
        var result = new FinancialMetricCalculator().Calculate(new FinancialMetricInput(
            200m, 100m, 30m, 20m, 150m, 200m, 80m, 300m, 90m, 180m, 60m, 50m, 10m));

        result.Roe.Should().Be(20m);
        result.Roic.Should().Be(15m);
        result.GrossMargin.Should().Be(40m);
        result.RevenueGrowth.Should().Be(100m);
        result.FreeCashFlow.Should().Be(40m);
        result.CurrentRatio.Should().Be(3m);
    }

    [Fact]
    public void FinancialMetricCalculator_ReturnsNullForUndefinedRatios()
    {
        var result = new FinancialMetricCalculator().Calculate(new FinancialMetricInput(null, 0m, 10m, 0m, 0m, 0m, null, 100m, null, 10m, 0m, 20m, 5m));

        result.Roe.Should().BeNull();
        result.RevenueGrowth.Should().BeNull();
        result.CurrentRatio.Should().BeNull();
    }

    [Fact]
    public void RuleBasedScoreCalculator_UsesDatabaseRuleBoundaries()
    {
        var rules = new[]
        {
            new InvestmentIndicatorRule("B01", 20m, 10m, 25m, null, 10m, 1),
            new InvestmentIndicatorRule("B01", 20m, 10m, 20m, 25m, 9m, 2),
            new InvestmentIndicatorRule("B01", 20m, 10m, null, 20m, 5m, 3)
        };

        var result = new RuleBasedScoreCalculator().Calculate(new Dictionary<string, decimal?> { ["B01"] = 25m }, rules);

        result.Score.Should().Be(100m);
        result.Components["B01"].Should().Be(100m);
    }

    [Fact]
    public void RuleBasedScoreCalculator_UsesIndicatorWeights()
    {
        var rules = new[]
        {
            new InvestmentIndicatorRule("B01", 3m, 10m, null, null, 10m, 1),
            new InvestmentIndicatorRule("B02", 1m, 10m, null, null, 0m, 1)
        };

        var result = new RuleBasedScoreCalculator().Calculate(
            new Dictionary<string, decimal?> { ["B01"] = 1m, ["B02"] = 1m },
            rules);

        result.Score.Should().Be(75m);
    }

    private static FinancialSnapshot CreateExcellentSnapshot() => new("600519", new DateOnly(2026, 8, 26), 25m, 18m, 55m, 30m, 1.2m, 25m, 2.5m, 15m, 1.2m, 2m, 12m, 18m, 25m, 22m, 10m);
}
