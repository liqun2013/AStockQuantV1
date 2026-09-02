using System.Text.Json;

namespace AStockQuant.Application.Interfaces;

public interface IAkToolsClient
{
		Task<JsonDocument> GetAsync(string interfaceName, IReadOnlyDictionary<string, string?> parameters, CancellationToken cancellationToken = default);
}
