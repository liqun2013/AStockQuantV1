using AStockQuant.Application.Screening;

namespace AStockQuant.Application.Interfaces;

public interface IScreeningRepository
{
		Task<ScreeningProfile?> GetProfileByCodeAndVersionAsync(string profileCode, string version, CancellationToken cancellationToken);
}
