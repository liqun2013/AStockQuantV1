using AStockQuant.Application.Interfaces;
using AStockQuant.Application.Screening;
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

	private sealed record ScreeningProfileRow(int ProfileId, string ProfileCode, string ProfileName, string Version, int ScoreModelId, bool IsActive);
	private sealed record ScreeningRuleRow(string RuleCode, decimal? NumericValue, bool? BoolValue, string? StringValue);
}
