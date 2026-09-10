namespace AStockQuant.Domain.Screening;

public sealed class IndustryRuleSet(IReadOnlyList<IndustryScreeningRuleOverride> overrides)
{
		private static readonly HashSet<string> SupportedRuleCodes = new(StringComparer.OrdinalIgnoreCase)
		{
				"MINIMUM_BUFFETT_SCORE",
				"MINIMUM_GRAHAM_SCORE",
				"MINIMUM_FISHER_SCORE",
				"MINIMUM_FINAL_SCORE"
		};

		public ScreeningProfile GetEffectiveProfile(ScreeningProfile profile, string? industryCode)
		{
		foreach (var overrideRule in overrides) Validate(overrideRule);
				if (string.IsNullOrWhiteSpace(industryCode)) return profile;

				var overridesByRule = overrides
						.Where(overrideRule => overrideRule.IndustryCode.Equals(industryCode, StringComparison.OrdinalIgnoreCase))
						.ToDictionary(overrideRule => overrideRule.RuleCode, StringComparer.OrdinalIgnoreCase);
				if (overridesByRule.Count == 0) return profile;

				var effectiveRules = profile.Rules
						.Select(rule => overridesByRule.TryGetValue(rule.RuleCode, out var overrideRule)
								? rule with { NumericValue = overrideRule.NumericValue, BoolValue = null, StringValue = null }
								: rule)
						.ToArray();
				return new ScreeningProfile(profile.ProfileId, profile.ProfileCode, profile.ProfileName, profile.Version, profile.ScoreModelId, profile.IsActive, effectiveRules);
		}

		public static void Validate(IndustryScreeningRuleOverride overrideRule)
		{
				if (!SupportedRuleCodes.Contains(overrideRule.RuleCode))
						throw new InvalidOperationException($"Industry override rule '{overrideRule.RuleCode}' is not supported.");
				if (overrideRule.NumericValue is < 0m or > 100m)
						throw new InvalidOperationException($"Industry override rule '{overrideRule.RuleCode}' must be between 0 and 100.");
		}
}