using AStockQuant.Domain.Screening;
using FluentAssertions;
using Xunit;

namespace AStockQuant.Domain.Tests;

public sealed class ScreeningProfileTests
{
		[Fact]
		public void Decide_RejectsStStock_WhenExclusionIsEnabled()
		{
				var decision = CreateProfile(excludeSt: true).Decide(CreateContext(isSt: true));

				decision.IsSelected.Should().BeFalse();
				decision.RejectionReason.Should().Be("Stock is ST.");
		}

		[Fact]
		public void Decide_SelectsEligibleNonStStock()
		{
				var decision = CreateProfile(excludeSt: true).Decide(CreateContext(isSt: false));

				decision.IsSelected.Should().BeTrue();
				decision.RejectionReason.Should().BeNull();
		}

		private static ScreeningProfile CreateProfile(bool excludeSt) => new(1, "VALUE_QUALITY", "Test", "V1.0", 1, true,
		[
				new("TOP_N", 50m, null, null),
				new("MINIMUM_LISTING_YEARS", 3m, null, null),
				new("MINIMUM_FISHER_SCORE", 60m, null, null),
				new("MINIMUM_BUFFETT_SCORE", 60m, null, null),
				new("MINIMUM_GRAHAM_SCORE", 50m, null, null),
				new("REQUIRE_COMPLETE_FINANCIAL_DATA", null, true, null),
				new("EXCLUDE_ST", null, excludeSt, null)
		]);

		private static ScreeningContext CreateContext(bool isSt) => new(1, "600519", "Test", null, null, new DateOnly(2026, 8, 26), new DateOnly(2020, 1, 1), new DateOnly(2025, 12, 31), 70m, 60m, 80m, 75m, true, isSt, true);
}