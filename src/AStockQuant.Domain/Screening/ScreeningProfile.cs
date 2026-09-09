namespace AStockQuant.Domain.Screening;

public sealed class ScreeningProfile
{
		private const string TopNCode = "TOP_N";
		private const string MinimumListingYearsCode = "MINIMUM_LISTING_YEARS";
		private const string MinimumFisherScoreCode = "MINIMUM_FISHER_SCORE";
		private const string MinimumBuffettScoreCode = "MINIMUM_BUFFETT_SCORE";
		private const string MinimumGrahamScoreCode = "MINIMUM_GRAHAM_SCORE";
		private const string MinimumFinalScoreCode = "MINIMUM_FINAL_SCORE";
		private const string RequireCompleteFinancialDataCode = "REQUIRE_COMPLETE_FINANCIAL_DATA";
		private const string ExcludeStCode = "EXCLUDE_ST";

		private readonly IReadOnlyDictionary<string, ScreeningRule> rules;

		public ScreeningProfile(int profileId, string profileCode, string profileName, string version, int scoreModelId, bool isActive, IReadOnlyList<ScreeningRule> rules)
		{
				ProfileId = profileId;
				ProfileCode = profileCode;
				ProfileName = profileName;
				Version = version;
				ScoreModelId = scoreModelId;
				IsActive = isActive;
				this.rules = rules.ToDictionary(rule => rule.RuleCode, StringComparer.OrdinalIgnoreCase);
				TopN = GetInteger(TopNCode, 1, 500);
				MinimumListingYears = GetInteger(MinimumListingYearsCode, 0, 50);
				MinimumFisherScore = GetNumber(MinimumFisherScoreCode, 0, 100);
				MinimumBuffettScore = GetNumber(MinimumBuffettScoreCode, 0, 100);
				MinimumGrahamScore = GetNumber(MinimumGrahamScoreCode, 0, 100);
				MinimumFinalScore = GetNumber(MinimumFinalScoreCode, 0, 100);
				RequireCompleteFinancialData = GetBoolean(RequireCompleteFinancialDataCode);
				ExcludeST = GetBoolean(ExcludeStCode);
		}

		public int ProfileId { get; }
		public string ProfileCode { get; }
		public string ProfileName { get; }
		public string Version { get; }
		public int ScoreModelId { get; }
		public bool IsActive { get; }
		public int TopN { get; }
		public int MinimumListingYears { get; }
		public decimal MinimumFisherScore { get; }
		public decimal MinimumBuffettScore { get; }
		public decimal MinimumGrahamScore { get; }
		public decimal MinimumFinalScore { get; }
		public bool RequireCompleteFinancialData { get; }
		public bool ExcludeST { get; }

		public ScreeningDecision Decide(ScreeningContext context)
		{
				if (!context.IsActive) return Reject(context, "Stock is inactive.");
				if (context.ListingDate is null || context.ListingDate > context.ScoreDate.AddYears(-MinimumListingYears)) return Reject(context, "Stock has not met the minimum listing period.");
				if (ExcludeST && context.IsST) return Reject(context, "Stock is ST.");
				if (context.BuffettScore < MinimumBuffettScore) return Reject(context, "Buffett score is below the minimum.");
				if (context.GrahamScore < MinimumGrahamScore) return Reject(context, "Graham score is below the minimum.");
				if (context.FisherScore < MinimumFisherScore) return Reject(context, "Fisher score is below the minimum.");
				if (context.FinalScore < MinimumFinalScore) return Reject(context, "Final score is below the minimum.");
				if (RequireCompleteFinancialData && !context.HasCompleteFinancialData) return Reject(context, "Financial data is incomplete.");
				return new ScreeningDecision(context, true, null);
		}

		private static ScreeningDecision Reject(ScreeningContext context, string reason) => new(context, false, reason);

		private decimal GetNumber(string ruleCode, decimal minimum, decimal maximum)
		{
				var value = GetRule(ruleCode);
				if (value.NumericValue is null || value.BoolValue is not null || value.StringValue is not null || value.NumericValue < minimum || value.NumericValue > maximum)
						throw new InvalidOperationException($"Screening rule '{ruleCode}' must define one numeric value between {minimum} and {maximum}.");
				return value.NumericValue.Value;
		}

		private int GetInteger(string ruleCode, int minimum, int maximum)
		{
				var value = GetNumber(ruleCode, minimum, maximum);
				if (value != decimal.Truncate(value)) throw new InvalidOperationException($"Screening rule '{ruleCode}' must be a whole number.");
				return decimal.ToInt32(value);
		}

		private bool GetBoolean(string ruleCode)
		{
				var value = GetRule(ruleCode);
				if (value.BoolValue is null || value.NumericValue is not null || value.StringValue is not null)
						throw new InvalidOperationException($"Screening rule '{ruleCode}' must define one boolean value.");
				return value.BoolValue.Value;
		}

		private ScreeningRule GetRule(string ruleCode) => rules.TryGetValue(ruleCode, out var value)
				? value
				: throw new InvalidOperationException($"Screening rule '{ruleCode}' is required.");
}