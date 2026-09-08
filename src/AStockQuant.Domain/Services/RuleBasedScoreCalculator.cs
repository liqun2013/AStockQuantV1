using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Domain.Services;

public sealed class RuleBasedScoreCalculator
{
		public ScoreResult Calculate(IReadOnlyDictionary<string, decimal?> indicators, IReadOnlyCollection<InvestmentIndicatorRule> rules)
		{
				var components = new Dictionary<string, decimal>();
				var totalWeight = 0m;
				var weightedScore = 0m;
				foreach (var group in rules.GroupBy(rule => rule.IndicatorCode, StringComparer.OrdinalIgnoreCase))
				{
						if (!indicators.TryGetValue(group.Key, out var value) || !value.HasValue) continue;

						var rule = group.OrderBy(item => item.RuleOrder).FirstOrDefault(item =>
								(!item.MinValue.HasValue || value.Value >= item.MinValue.Value) &&
								(!item.MaxValue.HasValue || value.Value < item.MaxValue.Value));
						if (rule is null || rule.MaxScore <= 0m) continue;
						var componentScore = ScoreMath.Clamp(rule.Score / rule.MaxScore * 100m);
						components[group.Key] = componentScore;
						if (rule.Weight > 0m)
						{
							weightedScore += componentScore * rule.Weight;
							totalWeight += rule.Weight;
						}
				}

				var score = totalWeight <= 0m
						? 0m
						: Math.Round(weightedScore / totalWeight, 4, MidpointRounding.AwayFromZero);
				return new ScoreResult(score, ScoreMath.Grade(score), components);
		}
}
