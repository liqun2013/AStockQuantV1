using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Domain.Services;

public sealed class RuleBasedScoreCalculator
{
		public ScoreResult Calculate(IReadOnlyDictionary<string, decimal?> indicators, IReadOnlyCollection<InvestmentIndicatorRule> rules)
		{
				var components = new Dictionary<string, decimal>();
				foreach (var group in rules.GroupBy(rule => rule.IndicatorCode, StringComparer.OrdinalIgnoreCase))
				{
						if (!indicators.TryGetValue(group.Key, out var value) || !value.HasValue) continue;

						var rule = group.OrderBy(item => item.RuleOrder).FirstOrDefault(item =>
								(!item.MinValue.HasValue || value.Value >= item.MinValue.Value) &&
								(!item.MaxValue.HasValue || value.Value < item.MaxValue.Value));
						if (rule is null || rule.MaxScore <= 0m) continue;
						components[group.Key] = ScoreMath.Clamp(rule.Score / rule.MaxScore * 100m);
				}

				var score = components.Count == 0
						? 0m
						: Math.Round(components.Values.Average(), 4, MidpointRounding.AwayFromZero);
				return new ScoreResult(score, ScoreMath.Grade(score), components);
		}
}
