using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Infrastructure.Persistence;
using Dapper;

namespace AStockQuant.Infrastructure.Repositories;

public sealed class FinancialIndicatorRepository(ISqlConnectionFactory connectionFactory) : IFinancialIndicatorRepository
{
		public async Task<SyncResult> CalculateAndUpsertAsync(string? stockCode = null, CancellationToken cancellationToken = default)
		{
				using var connection = connectionFactory.CreateConnection();
				connection.Open();
				using var transaction = connection.BeginTransaction();
				try
				{
						const string sql = """
;WITH FinancialData AS
(
		SELECT
				r.ReportId,
				r.StockId,
				LAG(r.ReportId) OVER (PARTITION BY r.StockId ORDER BY r.ReportPeriod, r.VersionNo) AS PreviousReportId,
				inc.Revenue,
				inc.NetProfit,
				inc.GrossProfit,
				inc.OperatingProfit,
				inc.EPS,
				bs.TotalAssets,
				bs.TotalLiabilities,
				bs.TotalEquity,
				bs.CurrentAssets,
				bs.CurrentLiabilities,
				cf.OperatingCashFlow,
				cf.CapitalExpenditure
		FROM Finance.FinancialReport r
		INNER JOIN Basic.Stock s ON s.StockId = r.StockId
		LEFT JOIN Finance.IncomeStatement inc ON inc.ReportId = r.ReportId
		LEFT JOIN Finance.BalanceSheet bs ON bs.ReportId = r.ReportId
		LEFT JOIN Finance.CashFlowStatement cf ON cf.ReportId = r.ReportId
		WHERE (@StockCode IS NULL OR s.StockCode = @StockCode)
), PreviousData AS
(
		SELECT
				currentData.*,
				previousData.Revenue AS PreviousRevenue,
				previousData.NetProfit AS PreviousNetProfit,
				previousData.TotalEquity AS PreviousEquity
		FROM FinancialData currentData
		LEFT JOIN FinancialData previousData ON previousData.ReportId = currentData.PreviousReportId
), Calculated AS
(
		SELECT
				ReportId,
				StockId,
				NULLIF(NetProfit, 0) / NULLIF((TotalEquity + PreviousEquity) / 2, 0) * 100 AS ROE,
				NULLIF(NetProfit, 0) / NULLIF(TotalAssets - TotalLiabilities, 0) * 100 AS ROIC,
				NULLIF(GrossProfit, 0) / NULLIF(Revenue, 0) * 100 AS GrossMargin,
				NULLIF(OperatingProfit, 0) / NULLIF(Revenue, 0) * 100 AS OperatingMargin,
				NULLIF(NetProfit, 0) / NULLIF(Revenue, 0) * 100 AS NetMargin,
				NULLIF(TotalLiabilities, 0) / NULLIF(TotalAssets, 0) * 100 AS DebtRatio,
				NULLIF(CurrentAssets, 0) / NULLIF(CurrentLiabilities, 0) AS CurrentRatio,
				NULLIF(Revenue - PreviousRevenue, 0) / NULLIF(ABS(PreviousRevenue), 0) * 100 AS RevenueGrowth,
				NULLIF(NetProfit - PreviousNetProfit, 0) / NULLIF(ABS(PreviousNetProfit), 0) * 100 AS NetProfitGrowth,
				NULLIF(OperatingCashFlow, 0) / NULLIF(NetProfit, 0) AS OperatingCashFlowToNetProfit,
				OperatingCashFlow - CapitalExpenditure AS FreeCashFlow,
				NULLIF(OperatingCashFlow - CapitalExpenditure, 0) / NULLIF(Revenue, 0) * 100 AS FreeCashFlowMargin
		FROM PreviousData
)
MERGE Finance.FinancialIndicator AS target
USING Calculated AS source ON target.ReportId = source.ReportId
WHEN MATCHED THEN UPDATE SET
		ROE=source.ROE, ROIC=source.ROIC, GrossMargin=source.GrossMargin, OperatingMargin=source.OperatingMargin,
		NetMargin=source.NetMargin, DebtRatio=source.DebtRatio, CurrentRatio=source.CurrentRatio,
		RevenueGrowth=source.RevenueGrowth, NetProfitGrowth=source.NetProfitGrowth,
		OperatingCashFlowToNetProfit=source.OperatingCashFlowToNetProfit, FreeCashFlow=source.FreeCashFlow,
		FreeCashFlowMargin=source.FreeCashFlowMargin
WHEN NOT MATCHED THEN INSERT
		(StockId, ReportId, ROE, ROIC, GrossMargin, OperatingMargin, NetMargin, DebtRatio, CurrentRatio, RevenueGrowth, NetProfitGrowth, OperatingCashFlowToNetProfit, FreeCashFlow, FreeCashFlowMargin, CreatedTime)
VALUES
		(source.StockId, source.ReportId, source.ROE, source.ROIC, source.GrossMargin, source.OperatingMargin, source.NetMargin, source.DebtRatio, source.CurrentRatio, source.RevenueGrowth, source.NetProfitGrowth, source.OperatingCashFlowToNetProfit, source.FreeCashFlow, source.FreeCashFlowMargin, SYSUTCDATETIME())
OUTPUT $action;
""";
						var actions = (await connection.QueryAsync<string>(new CommandDefinition(sql, new { StockCode = stockCode }, transaction, cancellationToken: cancellationToken))).AsList();
						transaction.Commit();
						var inserted = actions.Count(action => action.Equals("INSERT", StringComparison.OrdinalIgnoreCase));
						var updated = actions.Count - inserted;
						return new SyncResult("FinancialIndicator", actions.Count, actions.Count, 0, 0);
				}
				catch (Exception exception)
				{
						transaction.Rollback();
						return new SyncResult("FinancialIndicator", 0, 0, 0, 0, exception.Message);
				}
		}
}
