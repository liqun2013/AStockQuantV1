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

		[Theory]
		[InlineData(69, false)]
		[InlineData(70, true)]
		[InlineData(80, true)]
		public void Decide_AppliesMinimumFinalScore(decimal finalScore, bool isSelected)
		{
				var decision = CreateProfile(excludeSt: true, minimumFinalScore: 70m).Decide(CreateContext(isSt: false, finalScore));

				decision.IsSelected.Should().Be(isSelected);
				if (!isSelected) decision.RejectionReason.Should().Be("Final score is below the minimum.");
		}

		private static ScreeningProfile CreateProfile(bool excludeSt, decimal minimumFinalScore = 0m) => new(1, "VALUE_QUALITY", "Test", "V1.0", 1, true,
		[
				new("TOP_N", 50m, null, null),
				new("MINIMUM_LISTING_YEARS", 3m, null, null),
				new("MINIMUM_FISHER_SCORE", 60m, null, null),
				new("MINIMUM_BUFFETT_SCORE", 60m, null, null),
				new("MINIMUM_GRAHAM_SCORE", 50m, null, null),
				new("MINIMUM_FINAL_SCORE", minimumFinalScore, null, null),
				new("REQUIRE_COMPLETE_FINANCIAL_DATA", null, true, null),
				new("EXCLUDE_ST", null, excludeSt, null)
		]);

		private static ScreeningContext CreateContext(bool isSt, decimal finalScore = 75m) => new(1, "600519", "Test", null, null, new DateOnly(2026, 8, 26), new DateOnly(2020, 1, 1), new DateOnly(2025, 12, 31), 70m, 60m, 80m, finalScore, true, isSt, true);
}