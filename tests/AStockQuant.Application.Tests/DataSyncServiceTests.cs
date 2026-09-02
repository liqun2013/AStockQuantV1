using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Application.Services;
using FluentAssertions;
using Xunit;

namespace AStockQuant.Application.Tests;

public sealed class DataSyncServiceTests
{
		[Fact]
		public async Task SynchronizeAsync_ProcessesOneStockThroughAllStages()
		{
				var service = new DataSyncService(
						new FakeMarketProvider(),
						new FakeMarketRepository(),
						new FakeFinancialProvider(),
						new FakeFinancialRepository(),
						new FakeIndicatorRepository(),
						new FakeLogRepository());

				var result = await service.SynchronizeAsync(new SyncRequest("600519", 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2)));

				result.Succeeded.Should().BeTrue();
				result.StocksProcessed.Should().Be(1);
				result.PriceRowsProcessed.Should().Be(1);
				result.FinancialReportsProcessed.Should().Be(1);
				result.Errors.Should().BeEmpty();
		}

		private sealed class FakeMarketProvider : IMarketDataProvider
		{
				public Task<IReadOnlyList<StockImportDto>> GetStocksAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<StockImportDto>>([new("600519", "Test", "SSE", "Stock", "Main", null, true)]);
				public Task<IReadOnlyList<DailyPriceImportDto>> GetDailyPricesAsync(string stockCode, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<DailyPriceImportDto>>([new(stockCode, startDate, 1, 2, 0.5m, 1.5m, 1.2m, 0.3m, 25, 100, 150, 1, null, null, false, "Test")]);
		}

		private sealed class FakeFinancialProvider : IFinancialDataProvider
		{
				public Task<IReadOnlyList<FinancialReportImportDto>> GetReportsAsync(string stockCode, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<FinancialReportImportDto>>([new(stockCode, new DateOnly(2025, 12, 31), "Annual", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 1), "Test", 100, 60, 40, 20, 15, 1, 200, 80, 120, 100, 50, 20, 5)]);
		}

		private sealed class FakeMarketRepository : IMarketDataRepository
		{
				public Task<SyncResult> UpsertStocksAsync(IReadOnlyCollection<StockImportDto> stocks, CancellationToken cancellationToken = default) => Task.FromResult(new SyncResult("Stock", stocks.Count, stocks.Count, 0, 0));
				public Task<SyncResult> UpsertDailyPricesAsync(IReadOnlyCollection<DailyPriceImportDto> prices, CancellationToken cancellationToken = default) => Task.FromResult(new SyncResult("DailyPrice", prices.Count, prices.Count, 0, 0));
		}

		private sealed class FakeFinancialRepository : IFinancialDataRepository
		{
				public Task<SyncResult> UpsertReportsAsync(IReadOnlyCollection<FinancialReportImportDto> reports, CancellationToken cancellationToken = default) => Task.FromResult(new SyncResult("FinancialReport", reports.Count, reports.Count, 0, 0));
		}

		private sealed class FakeIndicatorRepository : IFinancialIndicatorRepository
		{
				public Task<SyncResult> CalculateAndUpsertAsync(string? stockCode = null, CancellationToken cancellationToken = default) => Task.FromResult(new SyncResult("FinancialIndicator", 1, 1, 0, 0));
		}

		private sealed class FakeLogRepository : IImportLogRepository
		{
				public Task<ImportLogHandle> StartAsync(string dataType, int? dataSourceId = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImportLogHandle(1, dataType));
				public Task CompleteAsync(ImportLogHandle handle, SyncResult result, CancellationToken cancellationToken = default) => Task.CompletedTask;
		}
}
