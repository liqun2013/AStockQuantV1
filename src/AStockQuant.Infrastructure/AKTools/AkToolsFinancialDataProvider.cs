using System.Globalization;
using System.Text.Json;
using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;

namespace AStockQuant.Infrastructure.AKTools;

public sealed class AkToolsFinancialDataProvider(IAkToolsClient client) : IFinancialDataProvider
{
		public async Task<IReadOnlyList<FinancialReportImportDto>> GetReportsAsync(string stockCode, CancellationToken cancellationToken = default)
		{
				var symbol = stockCode.EndsWith(".SH", StringComparison.OrdinalIgnoreCase) || stockCode.EndsWith(".SZ", StringComparison.OrdinalIgnoreCase)
						? stockCode
						: $"{stockCode}.{(stockCode.StartsWith('6') ? "SH" : "SZ")}";
				var reports = new Dictionary<DateOnly, ReportBuilder>();
				await LoadAsync("stock_profit_sheet_by_report_em", symbol, reports, Profit, cancellationToken);
				await LoadAsync("stock_balance_sheet_by_report_em", symbol, reports, Balance, cancellationToken);
				await LoadAsync("stock_cash_flow_sheet_by_report_em", symbol, reports, CashFlow, cancellationToken);

				return reports.Values
						.Select(report => report.Build(stockCode))
						.OrderByDescending(report => report.ReportPeriod)
						.ToArray();
		}

		private async Task LoadAsync(string interfaceName, string symbol, Dictionary<DateOnly, ReportBuilder> reports, Action<ReportBuilder, JsonElement> mapper, CancellationToken cancellationToken)
		{
				using var document = await client.GetAsync(interfaceName, new Dictionary<string, string?> { ["symbol"] = symbol }, cancellationToken);
				foreach (var row in Rows(document.RootElement))
				{
						var reportDate = Date(row, "REPORT_DATE", "报告期");
						if (!reportDate.HasValue) continue;
						var report = reports.TryGetValue(reportDate.Value, out var existing) ? existing : reports[reportDate.Value] = new ReportBuilder(reportDate.Value);
						report.ReportType = Text(row, "REPORT_TYPE", "报告类型") ?? report.ReportType;
						report.PublishDate = Date(row, "ANN_DATE", "公告日期", "PUBLISH_DATE") ?? report.PublishDate;
						mapper(report, row);
				}
		}

		private static void Profit(ReportBuilder report, JsonElement row)
		{
				report.Revenue ??= Number(row, "OPERATE_INCOME", "营业收入", "TOTAL_OPERATE_INCOME");
				report.OperatingCost ??= Number(row, "OPERATE_COST", "营业成本");
				report.GrossProfit ??= Number(row, "OPERATE_INCOME", "营业收入") - Number(row, "OPERATE_COST", "营业成本");
				report.OperatingProfit ??= Number(row, "OPERATE_PROFIT", "营业利润");
				report.NetProfit ??= Number(row, "NETPROFIT", "净利润", "NET_PROFIT");
				report.EarningsPerShare ??= Number(row, "BASIC_EPS", "基本每股收益", "EPS");
		}

		private static void Balance(ReportBuilder report, JsonElement row)
		{
				report.TotalAssets ??= Number(row, "TOTAL_ASSETS", "资产总计");
				report.TotalLiabilities ??= Number(row, "TOTAL_LIABILITIES", "负债合计");
				report.TotalEquity ??= Number(row, "TOTAL_EQUITY", "所有者权益合计");
				report.CurrentAssets ??= Number(row, "TOTAL_CURRENT_ASSETS", "流动资产合计");
				report.CurrentLiabilities ??= Number(row, "TOTAL_CURRENT_LIAB", "流动负债合计");
		}

		private static void CashFlow(ReportBuilder report, JsonElement row)
		{
				report.OperatingCashFlow ??= Number(row, "NETCASH_OPERATE", "经营活动产生的现金流量净额");
				report.CapitalExpenditure ??= Number(row, "CONSTRUCT_LONG_ASSET", "购建固定资产、无形资产和其他长期资产支付的现金");
		}

		private static IEnumerable<JsonElement> Rows(JsonElement root) => root.ValueKind switch
		{
				JsonValueKind.Array => root.EnumerateArray(),
				JsonValueKind.Object when root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array => data.EnumerateArray(),
				_ => []
		};

		private static string? Text(JsonElement row, params string[] names)
		{
				foreach (var name in names)
						if (row.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null)
								return value.ToString().Trim();
				return null;
		}

		private static decimal? Number(JsonElement row, params string[] names)
		{
				var text = Text(row, names);
				return decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : null;
		}

		private static DateOnly? Date(JsonElement row, params string[] names)
		{
				var text = Text(row, names);
				return DateOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value) ? value : null;
		}

		private sealed class ReportBuilder(DateOnly reportPeriod)
		{
				public DateOnly ReportPeriod { get; } = reportPeriod;
				public string ReportType { get; set; } = "Annual";
				public DateOnly? PublishDate { get; set; }
				public decimal? Revenue { get; set; }
				public decimal? OperatingCost { get; set; }
				public decimal? GrossProfit { get; set; }
				public decimal? OperatingProfit { get; set; }
				public decimal? NetProfit { get; set; }
				public decimal? EarningsPerShare { get; set; }
				public decimal? TotalAssets { get; set; }
				public decimal? TotalLiabilities { get; set; }
				public decimal? TotalEquity { get; set; }
				public decimal? CurrentAssets { get; set; }
				public decimal? CurrentLiabilities { get; set; }
				public decimal? OperatingCashFlow { get; set; }
				public decimal? CapitalExpenditure { get; set; }

				public FinancialReportImportDto Build(string stockCode) => new(stockCode, ReportPeriod, ReportType, PublishDate, PublishDate, "AKTools", Revenue, OperatingCost, GrossProfit, OperatingProfit, NetProfit, EarningsPerShare, TotalAssets, TotalLiabilities, TotalEquity, CurrentAssets, CurrentLiabilities, OperatingCashFlow, CapitalExpenditure);
		}
}
