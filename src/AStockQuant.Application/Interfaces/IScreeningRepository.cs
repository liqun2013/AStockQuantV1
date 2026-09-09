using AStockQuant.Domain.Screening;

namespace AStockQuant.Application.Interfaces;

public interface IScreeningRepository
{
		Task<ScreeningProfile?> GetProfileByCodeAndVersionAsync(string profileCode, string version, CancellationToken cancellationToken);
		Task<IReadOnlyList<ScreeningContext>> GetCandidateContextsAsync(int scoreModelId, DateOnly scoreDate, CancellationToken cancellationToken);
}
