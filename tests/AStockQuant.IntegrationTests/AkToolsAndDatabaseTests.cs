using System.Data;
using Microsoft.Data.SqlClient;
using Xunit;

namespace AStockQuant.IntegrationTests;

public sealed class AkToolsAndDatabaseTests
{
		private static bool Enabled => string.Equals(Environment.GetEnvironmentVariable("ASTOCKQUANT_RUN_INTEGRATION_TESTS"), "true", StringComparison.OrdinalIgnoreCase);
		private static string AkToolsBaseUrl => Environment.GetEnvironmentVariable("ASTOCKQUANT_AKTOOLS_URL") ?? "http://localhost:44802";
		private static string ConnectionString => Environment.GetEnvironmentVariable("ASTOCKQUANT_CONNECTION_STRING") ?? "Server=localhost,44801;Database=AStockQuant;User Id=sa;Password=Trialdata20210922*;TrustServerCertificate=True;";

		[Fact]
		public async Task AkTools_HistoricalData_ReturnsRowsFor002714()
		{
				if (!Enabled) return;
				using var client = new HttpClient { BaseAddress = new Uri(AkToolsBaseUrl) };
				using var response = await client.GetAsync("/api/public/stock_zh_a_hist_tx?symbol=sz002714&start_date=20260101&end_date=20260131");
				response.EnsureSuccessStatusCode();
				var content = await response.Content.ReadAsStringAsync();
				Assert.Contains("2026-01-30", content, StringComparison.Ordinal);
		}

		[Fact]
		public async Task Database_ContainsSuccessful002714Sync()
		{
				if (!Enabled) return;
				await using var connection = new SqlConnection(ConnectionString);
				await connection.OpenAsync();
				await using var command = connection.CreateCommand();
				command.CommandText = """
DECLARE @StockId INT = (SELECT StockId FROM Basic.Stock WHERE StockCode = '002714');
SELECT CONCAT(
		(SELECT COUNT(*) FROM Market.StockDailyPrice WHERE StockId = @StockId AND TradeDate BETWEEN '2026-01-01' AND '2026-01-31'), '|',
		(SELECT COUNT(*) FROM Finance.FinancialReport WHERE StockId = @StockId), '|',
		(SELECT COUNT(*) FROM Finance.FinancialIndicator i INNER JOIN Finance.FinancialReport r ON r.ReportId = i.ReportId WHERE r.StockId = @StockId));
""";
				var value = Convert.ToString(await command.ExecuteScalarAsync());
				Assert.Equal("20|59|59", value);
		}

}
