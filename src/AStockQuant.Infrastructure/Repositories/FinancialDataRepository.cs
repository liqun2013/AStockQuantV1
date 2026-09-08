using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Infrastructure.Persistence;
using Dapper;
using System.Text.Json;

namespace AStockQuant.Infrastructure.Repositories;

public sealed class FinancialDataRepository(ISqlConnectionFactory connectionFactory) : IFinancialDataRepository
{
		public async Task<SyncResult> UpsertReportsAsync(IReadOnlyCollection<FinancialReportImportDto> reports, CancellationToken cancellationToken = default)
		{
				using var connection = connectionFactory.CreateConnection();
				connection.Open();
				using var transaction = connection.BeginTransaction();
				var succeeded = 0;
				try
				{
						const string sql = """
SELECT * INTO #Source FROM OPENJSON(@ReportsJson) WITH
(
	StockCode VARCHAR(20), ReportPeriod DATE, ReportType VARCHAR(20), PublishDate DATE, AnnouncementDate DATE, Source VARCHAR(50),
	Revenue DECIMAL(24,8), OperatingCost DECIMAL(24,8), GrossProfit DECIMAL(24,8), OperatingProfit DECIMAL(24,8), NetProfit DECIMAL(24,8), EarningsPerShare DECIMAL(24,8),
	TotalAssets DECIMAL(24,8), TotalLiabilities DECIMAL(24,8), TotalEquity DECIMAL(24,8), CurrentAssets DECIMAL(24,8), CurrentLiabilities DECIMAL(24,8), OperatingCashFlow DECIMAL(24,8), CapitalExpenditure DECIMAL(24,8)
);
MERGE Finance.FinancialReport AS target
USING (SELECT stock.StockId, source.* FROM #Source source INNER JOIN Basic.Stock stock ON stock.StockCode = source.StockCode) AS source
ON target.StockId = source.StockId AND target.ReportPeriod = source.ReportPeriod AND target.ReportType = source.ReportType AND target.VersionNo = 1
WHEN MATCHED THEN UPDATE SET PublishDate = source.PublishDate, AnnouncementDate = source.AnnouncementDate, Source = source.Source, UpdatedTime = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (StockId, ReportPeriod, ReportType, PublishDate, AnnouncementDate, IsRestated, VersionNo, Source, CreatedTime, UpdatedTime)
VALUES (source.StockId, source.ReportPeriod, source.ReportType, source.PublishDate, source.AnnouncementDate, 0, 1, source.Source, SYSUTCDATETIME(), SYSUTCDATETIME());
MERGE Finance.IncomeStatement AS target
USING (SELECT report.ReportId, source.* FROM #Source source INNER JOIN Basic.Stock stock ON stock.StockCode = source.StockCode INNER JOIN Finance.FinancialReport report ON report.StockId = stock.StockId AND report.ReportPeriod = source.ReportPeriod AND report.ReportType = source.ReportType AND report.VersionNo = 1) AS source ON target.ReportId = source.ReportId
WHEN MATCHED THEN UPDATE SET Revenue=source.Revenue, OperatingCost=source.OperatingCost, GrossProfit=source.GrossProfit, OperatingProfit=source.OperatingProfit, NetProfit=source.NetProfit, EPS=source.EarningsPerShare, BasicEPS=source.EarningsPerShare
WHEN NOT MATCHED THEN INSERT (ReportId, Revenue, OperatingCost, GrossProfit, OperatingProfit, NetProfit, EPS, BasicEPS, CreatedTime) VALUES (source.ReportId, source.Revenue, source.OperatingCost, source.GrossProfit, source.OperatingProfit, source.NetProfit, source.EarningsPerShare, source.EarningsPerShare, SYSUTCDATETIME());
MERGE Finance.BalanceSheet AS target
USING (SELECT report.ReportId, source.* FROM #Source source INNER JOIN Basic.Stock stock ON stock.StockCode = source.StockCode INNER JOIN Finance.FinancialReport report ON report.StockId = stock.StockId AND report.ReportPeriod = source.ReportPeriod AND report.ReportType = source.ReportType AND report.VersionNo = 1) AS source ON target.ReportId = source.ReportId
WHEN MATCHED THEN UPDATE SET TotalAssets=source.TotalAssets, TotalLiabilities=source.TotalLiabilities, TotalEquity=source.TotalEquity, CurrentAssets=source.CurrentAssets, CurrentLiabilities=source.CurrentLiabilities
WHEN NOT MATCHED THEN INSERT (ReportId, TotalAssets, TotalLiabilities, TotalEquity, CurrentAssets, CurrentLiabilities, CreatedTime) VALUES (source.ReportId, source.TotalAssets, source.TotalLiabilities, source.TotalEquity, source.CurrentAssets, source.CurrentLiabilities, SYSUTCDATETIME());
MERGE Finance.CashFlowStatement AS target
USING (SELECT report.ReportId, source.* FROM #Source source INNER JOIN Basic.Stock stock ON stock.StockCode = source.StockCode INNER JOIN Finance.FinancialReport report ON report.StockId = stock.StockId AND report.ReportPeriod = source.ReportPeriod AND report.ReportType = source.ReportType AND report.VersionNo = 1) AS source ON target.ReportId = source.ReportId
WHEN MATCHED THEN UPDATE SET OperatingCashFlow=source.OperatingCashFlow, CapitalExpenditure=source.CapitalExpenditure
WHEN NOT MATCHED THEN INSERT (ReportId, OperatingCashFlow, CapitalExpenditure, CreatedTime) VALUES (source.ReportId, source.OperatingCashFlow, source.CapitalExpenditure, SYSUTCDATETIME());
""";
						await connection.ExecuteAsync(new CommandDefinition(sql, new { ReportsJson = JsonSerializer.Serialize(reports) }, transaction, cancellationToken: cancellationToken));
						succeeded = reports.Count;
						transaction.Commit();
						return new SyncResult("FinancialReport", reports.Count, succeeded, 0, reports.Count - succeeded);
				}
				catch (Exception exception)
				{
						transaction.Rollback();
						return new SyncResult("FinancialReport", reports.Count, 0, 0, reports.Count, exception.Message);
				}
		}
}
