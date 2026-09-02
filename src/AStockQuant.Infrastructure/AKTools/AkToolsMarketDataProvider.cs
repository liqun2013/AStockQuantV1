using System.Globalization;
using System.Text.Json;
using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;

namespace AStockQuant.Infrastructure.AKTools;

public sealed class AkToolsMarketDataProvider(IAkToolsClient client) : IMarketDataProvider
{
		public async Task<IReadOnlyList<StockImportDto>> GetStocksAsync(CancellationToken cancellationToken = default)
		{
				using var document = await client.GetAsync("stock_zh_a_spot_em", new Dictionary<string, string?>(), cancellationToken);
				var result = new List<StockImportDto>();
				foreach (var row in Rows(document.RootElement))
				{
						var code = String(row, "SECURITY_CODE", "代码");
						var name = String(row, "SECURITY_NAME_ABBR", "名称");
						if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) continue;
						result.Add(new StockImportDto(code, name, Exchange(code), "Stock", String(row, "MARKET", "市场"), null, true));
				}
				return result;
		}

		public async Task<IReadOnlyList<DailyPriceImportDto>> GetDailyPricesAsync(string stockCode, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
		{
				var parameters = new Dictionary<string, string?>
				{
						["symbol"] = stockCode,
						["period"] = "daily",
						["start_date"] = startDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
						["end_date"] = endDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
						["adjust"] = ""
				};
				using var document = await client.GetAsync("stock_zh_a_hist", parameters, cancellationToken);
				var result = new List<DailyPriceImportDto>();
				foreach (var row in Rows(document.RootElement))
				{
						var date = Date(row, "日期", "Date");
						var close = Decimal(row, "收盘", "Close");
						if (!date.HasValue || !close.HasValue) continue;
						result.Add(new DailyPriceImportDto(
								stockCode,
								date.Value,
								Decimal(row, "开盘", "Open"),
								Decimal(row, "最高", "High"),
								Decimal(row, "最低", "Low"),
								close,
								Decimal(row, "昨收", "Previous Close"),
								Decimal(row, "涨跌额", "Change"),
								Decimal(row, "涨跌幅", "Change Percent"),
								Long(row, "成交量", "Volume"),
								Decimal(row, "成交额", "Amount"),
								Decimal(row, "换手率", "Turnover"),
								null,
								null,
								false,
								"AKTools"));
				}
				return result;
		}

		private static IEnumerable<JsonElement> Rows(JsonElement root) => root.ValueKind switch
		{
				JsonValueKind.Array => root.EnumerateArray(),
				JsonValueKind.Object when root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array => data.EnumerateArray(),
				_ => []
		};

		private static string Exchange(string code) => code.StartsWith('6') ? "SSE" : "SZSE";

		private static string? String(JsonElement row, params string[] names)
		{
				foreach (var name in names)
						if (row.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null)
								return value.ToString().Trim();
				return null;
		}

		private static decimal? Decimal(JsonElement row, params string[] names)
		{
				var value = String(row, names);
				return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
		}

		private static long? Long(JsonElement row, params string[] names)
		{
				var value = String(row, names);
				return long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
		}

		private static DateOnly? Date(JsonElement row, params string[] names)
		{
				var value = String(row, names);
				return DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : null;
		}
}
