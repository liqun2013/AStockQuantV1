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
				const string sqlProfile = "SELECT ProfileId, ProfileCode, ProfileName, Description, Version, ScoreModelId, IsActive FROM Strategy.ScreeningProfile WHERE ProfileCode = @ProfileCode AND Version = @Version";
				var profile = await connection.QuerySingleOrDefaultAsync(sqlProfile, new { ProfileCode = profileCode, Version = version });
				if (profile == null)
						return null;
				int profileId = profile.ProfileId;
				const string sqlRules = "SELECT RuleCode, NumericValue, BoolValue, StringValue FROM Strategy.ScreeningRule WHERE ProfileId = @ProfileId";
				var rules = (await connection.QueryAsync(sqlRules, new { ProfileId = profileId })).Select(r => new ScreeningRule((string)r.RuleCode, r.NumericValue == null ? null : (decimal?)r.NumericValue, r.BoolValue == null ? null : (bool?)r.BoolValue, r.StringValue == null ? null : (string)r.StringValue)).ToArray();
				return new ScreeningProfile(profileId, (string)profile.ProfileCode, (string)profile.ProfileName, (string)profile.Version, (int)profile.ScoreModelId, (bool)profile.IsActive, rules);
		}
}
