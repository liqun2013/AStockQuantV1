using AStockQuant.Domain.Screening;
using FluentAssertions;
using Xunit;

namespace AStockQuant.Domain.Tests;

public sealed class IndustryRuleSetTests
{
		[Fact]
		public void GetEffectiveProfile_OverridesIndustryThreshold()
		{
				var profile = CreateProfile();
				var ruleSet = new IndustryRuleSet([new IndustryScreeningRuleOverride("BANK", "MINIMUM_GRAHAM_SCORE", 45m)]);

				var effectiveProfile = ruleSet.GetEffectiveProfile(profile, "BANK");

				effectiveProfile.MinimumGrahamScore.Should().Be(45m);
				effectiveProfile.MinimumFisherScore.Should().Be(60m);
		}

		private static ScreeningProfile CreateProfile() => new(1, "VALUE_QUALITY", "Test", "V1.0", 1, true,
		[
				new("TOP_N", 50m, null, null), new("MINIMUM_LISTING_YEARS", 3m, null, null), new("MINIMUM_FISHER_SCORE", 60m, null, null),
				new("MINIMUM_BUFFETT_SCORE", 60m, null, null), new("MINIMUM_GRAHAM_SCORE", 50m, null, null), new("MINIMUM_FINAL_SCORE", 0m, null, null),
				new("REQUIRE_COMPLETE_FINANCIAL_DATA", null, true, null), new("EXCLUDE_ST", null, true, null)
		]);
}