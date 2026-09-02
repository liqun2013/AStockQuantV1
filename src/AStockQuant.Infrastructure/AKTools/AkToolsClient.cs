using System.Text.Json;
using AStockQuant.Application.Interfaces;

namespace AStockQuant.Infrastructure.AKTools;

public sealed class AkToolsClient(HttpClient httpClient) : IAkToolsClient
{
		public async Task<JsonDocument> GetAsync(string interfaceName, IReadOnlyDictionary<string, string?> parameters, CancellationToken cancellationToken = default)
		{
				if (string.IsNullOrWhiteSpace(interfaceName))
						throw new ArgumentException("AKTools interface name is required.", nameof(interfaceName));

				var query = string.Join("&", parameters
						.Where(pair => pair.Value is not null)
						.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value!)}"));
				var path = $"api/public/{Uri.EscapeDataString(interfaceName)}";
				if (query.Length > 0) path += $"?{query}";

				using var response = await httpClient.GetAsync(path, cancellationToken);
				response.EnsureSuccessStatusCode();
				return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
		}
}
