using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Infrastructure.Persistence;
using Dapper;

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
DECLARE @ReportId BIGINT;

SELECT @ReportId = r.ReportId
FROM Finance.FinancialReport r
INNER JOIN Basic.Stock s ON s.StockId = r.StockId
WHERE s.StockCode = @StockCode
	AND r.ReportPeriod = @ReportPeriod
	AND r.ReportType = @ReportType
	AND r.VersionNo = 1;

IF @ReportId IS NULL
BEGIN
		INSERT INTO Finance.FinancialReport (StockId, ReportPeriod, ReportType, PublishDate, AnnouncementDate, IsRestated, VersionNo, Source, CreatedTime, UpdatedTime)
		SELECT s.StockId, @ReportPeriod, @ReportType, @PublishDate, @AnnouncementDate, 0, 1, @Source, SYSUTCDATETIME(), SYSUTCDATETIME()
		FROM Basic.Stock s
		WHERE s.StockCode = @StockCode;
		SET @ReportId = SCOPE_IDENTITY();
END
ELSE
BEGIN
		UPDATE Finance.FinancialReport
		SET PublishDate = @PublishDate, AnnouncementDate = @AnnouncementDate, Source = @Source, UpdatedTime = SYSUTCDATETIME()
		WHERE ReportId = @ReportId;
END;

IF @ReportId IS NULL THROW 50001, 'Stock does not exist for financial report.', 1;

MERGE Finance.IncomeStatement AS target
USING (SELECT @ReportId AS ReportId) AS source ON target.ReportId = source.ReportId
WHEN MATCHED THEN UPDATE SET Revenue=@Revenue, OperatingCost=@OperatingCost, GrossProfit=@GrossProfit, OperatingProfit=@OperatingProfit, NetProfit=@NetProfit, EPS=@EarningsPerShare, BasicEPS=@EarningsPerShare
WHEN NOT MATCHED THEN INSERT (ReportId, Revenue, OperatingCost, GrossProfit, OperatingProfit, NetProfit, EPS, BasicEPS, CreatedTime)
VALUES (@ReportId, @Revenue, @OperatingCost, @GrossProfit, @OperatingProfit, @NetProfit, @EarningsPerShare, @EarningsPerShare, SYSUTCDATETIME());

MERGE Finance.BalanceSheet AS target
USING (SELECT @ReportId AS ReportId) AS source ON target.ReportId = source.ReportId
WHEN MATCHED THEN UPDATE SET TotalAssets=@TotalAssets, TotalLiabilities=@TotalLiabilities, TotalEquity=@TotalEquity, CurrentAssets=@CurrentAssets, CurrentLiabilities=@CurrentLiabilities
WHEN NOT MATCHED THEN INSERT (ReportId, TotalAssets, TotalLiabilities, TotalEquity, CurrentAssets, CurrentLiabilities, CreatedTime)
VALUES (@ReportId, @TotalAssets, @TotalLiabilities, @TotalEquity, @CurrentAssets, @CurrentLiabilities, SYSUTCDATETIME());

MERGE Finance.CashFlowStatement AS target
USING (SELECT @ReportId AS ReportId) AS source ON target.ReportId = source.ReportId
WHEN MATCHED THEN UPDATE SET OperatingCashFlow=@OperatingCashFlow, CapitalExpenditure=@CapitalExpenditure
WHEN NOT MATCHED THEN INSERT (ReportId, OperatingCashFlow, CapitalExpenditure, CreatedTime)
VALUES (@ReportId, @OperatingCashFlow, @CapitalExpenditure, SYSUTCDATETIME());
""";
						foreach (var report in reports)
						{
								var parameters = new
								{
										report.StockCode,
										ReportPeriod = report.ReportPeriod.ToDateTime(TimeOnly.MinValue),
										report.ReportType,
										PublishDate = report.PublishDate?.ToDateTime(TimeOnly.MinValue),
										AnnouncementDate = report.AnnouncementDate?.ToDateTime(TimeOnly.MinValue),
										report.Source,
										report.Revenue,
										report.OperatingCost,
										report.GrossProfit,
										report.OperatingProfit,
										report.NetProfit,
										report.EarningsPerShare,
										report.TotalAssets,
										report.TotalLiabilities,
										report.TotalEquity,
										report.CurrentAssets,
										report.CurrentLiabilities,
										report.OperatingCashFlow,
										report.CapitalExpenditure
								};
								await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
								succeeded++;
						}
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
