using AStockQuant.Application.DTOs;

namespace AStockQuant.Application.Interfaces;

public interface IScreeningService
{
		Task<IReadOnlyList<StockCandidateDto>> GetCandidatesAsync(string profileCode, string version, DateOnly? scoreDate, CancellationToken cancellationToken = default);
}