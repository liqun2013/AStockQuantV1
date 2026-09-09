using AStockQuant.Application.Interfaces;
using AStockQuant.Domain.Screening;
using AStockQuant.Infrastructure.Persistence;
using Dapper;

namespace AStockQuant.Infrastructure.Repositories;

public sealed class ScreeningRepository(ISqlConnectionFactory connectionFactory) : IScreeningRepository
{
	public async Task<ScreeningProfile?> GetProfileByCodeAndVersionAsync(string profileCode, string version, CancellationToken cancellationToken)
	{
		using var connection = connectionFactory.CreateConnection();
		const string profileSql = """
SELECT ProfileId, ProfileCode, ProfileName, Version, ScoreModelId, IsActive
FROM Strategy.ScreeningProfile
WHERE ProfileCode = @ProfileCode
	AND Version = @Version
	AND IsActive = 1;
""";
		var profile = await connection.QuerySingleOrDefaultAsync<ScreeningProfileRow>(new CommandDefinition(
			profileSql,
			new { ProfileCode = profileCode, Version = version },
			cancellationToken: cancellationToken));
		if (profile is null)
		{
			return null;
		}

		const string ruleSql = """
SELECT RuleCode, NumericValue, BoolValue, StringValue
FROM Strategy.ScreeningRule
WHERE ProfileId = @ProfileId;
""";
		var rules = await connection.QueryAsync<ScreeningRuleRow>(new CommandDefinition(
			ruleSql,
			new { profile.ProfileId },
			cancellationToken: cancellationToken));

		return new ScreeningProfile(
			profile.ProfileId,
			profile.ProfileCode,
			profile.ProfileName,
			profile.Version,
			profile.ScoreModelId,
			profile.IsActive,
			rules.Select(rule => new ScreeningRule(rule.RuleCode, rule.NumericValue, rule.BoolValue, rule.StringValue)).ToArray());
	}

	public async Task<IReadOnlyList<ScreeningContext>> GetCandidateContextsAsync(int scoreModelId, DateOnly scoreDate, CancellationToken cancellationToken)
	{
		using var connection = connectionFactory.CreateConnection();
		const string sql = """
SELECT
		stock.StockId,
		stock.StockCode,
		stock.StockName,
		industry.IndustryCode,
		industry.IndustryName,
		score.ScoreDate,
		stock.ListingDate,
		financial.ReportPeriod AS FinancialReportDate,
		score.BuffettScore,
		score.GrahamScore,
		score.FisherScore,
		score.FinalScore,
		stock.IsActive,
		stock.IsST,
		CAST(CASE WHEN financial.ROE IS NOT NULL AND financial.ROIC IS NOT NULL AND financial.GrossMargin IS NOT NULL
				AND financial.NetMargin IS NOT NULL AND financial.DebtRatio IS NOT NULL AND financial.CurrentRatio IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS HasCompleteFinancialData
FROM Quant.InvestmentScore score
INNER JOIN Basic.Stock stock ON stock.StockId = score.StockId
OUTER APPLY
(
		SELECT TOP (1) relation.IndustryId
		FROM Basic.StockIndustry relation
		WHERE relation.StockId = stock.StockId
			AND relation.IsPrimary = 1
			AND relation.EffectiveDate <= score.ScoreDate
			AND (relation.ExpireDate IS NULL OR relation.ExpireDate >= score.ScoreDate)
		ORDER BY relation.EffectiveDate DESC
) primaryIndustry
LEFT JOIN Basic.Industry industry ON industry.IndustryId = primaryIndustry.IndustryId
OUTER APPLY
(
		SELECT TOP (1) report.ReportPeriod, indicator.ROE, indicator.ROIC, indicator.GrossMargin,
				indicator.NetMargin, indicator.DebtRatio, indicator.CurrentRatio
		FROM Finance.FinancialReport report
		INNER JOIN Finance.FinancialIndicator indicator ON indicator.ReportId = report.ReportId
		WHERE report.StockId = stock.StockId
			AND report.PublishDate IS NOT NULL
			AND report.PublishDate <= score.ScoreDate
		ORDER BY report.ReportPeriod DESC, report.PublishDate DESC, report.VersionNo DESC
) financial
WHERE score.ModelId = @ScoreModelId
	AND score.ScoreDate = @ScoreDate
	AND (stock.DelistingDate IS NULL OR stock.DelistingDate > score.ScoreDate);
""";
		return (await connection.QueryAsync<ScreeningContext>(new CommandDefinition(sql, new
		{
			ScoreModelId = scoreModelId,
			ScoreDate = scoreDate.ToDateTime(TimeOnly.MinValue)
		}, cancellationToken: cancellationToken))).AsList();
	}

	private sealed record ScreeningProfileRow(int ProfileId, string ProfileCode, string ProfileName, string Version, int ScoreModelId, bool IsActive);
	private sealed record ScreeningRuleRow(string RuleCode, decimal? NumericValue, bool? BoolValue, string? StringValue);
}
