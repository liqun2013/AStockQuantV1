using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Domain.Services;

public sealed class FinancialMetricCalculator
{
		public FinancialMetrics Calculate(FinancialMetricInput input)
		{
				var freeCashFlow = input.OperatingCashFlow - input.CapitalExpenditure;
				return new FinancialMetrics(
						RatioAsPercent(input.NetProfit, input.AverageEquity),
						RatioAsPercent(input.NetProfit, input.InvestedCapital),
						RatioAsPercent(input.GrossProfit, input.Revenue),
						RatioAsPercent(input.NetProfit, input.Revenue),
						RatioAsPercent(input.TotalLiabilities, input.TotalAssets),
						Ratio(input.CurrentAssets, input.CurrentLiabilities),
						Ratio(input.OperatingCashFlow, input.NetProfit),
						freeCashFlow,
						RatioAsPercent(freeCashFlow, input.Revenue),
						GrowthAsPercent(input.Revenue, input.PreviousRevenue),
						GrowthAsPercent(input.NetProfit, input.PreviousNetProfit));
		}

		private static decimal? RatioAsPercent(decimal? numerator, decimal? denominator) =>
				numerator.HasValue && denominator.HasValue && denominator.Value != 0m
						? numerator.Value / denominator.Value * 100m
						: null;

		private static decimal? Ratio(decimal? numerator, decimal? denominator) =>
				numerator.HasValue && denominator.HasValue && denominator.Value != 0m
						? numerator.Value / denominator.Value
						: null;

		private static decimal? GrowthAsPercent(decimal? current, decimal? previous) =>
				current.HasValue && previous.HasValue && previous.Value != 0m
						? (current.Value - previous.Value) / Math.Abs(previous.Value) * 100m
						: null;
}
