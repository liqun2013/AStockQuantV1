using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Application.Services;
using FluentAssertions;
using Xunit;

namespace AStockQuant.Application.Tests;

public sealed class DataSyncServiceTests
{
		[Fact]
		public async Task SynchronizeAsync_ProcessesStocksThroughBatchStages()
		{
				var marketProvider = new FakeMarketProvider();
				var financialProvider = new FakeFinancialProvider();
				var indicatorRepository = new FakeIndicatorRepository();
				var service = new DataSyncService(
						marketProvider,
						new FakeMarketRepository(),
						financialProvider,
						new FakeFinancialRepository(),
						indicatorRepository,
						new FakeLogRepository());

				var result = await service.SynchronizeAsync(new SyncRequest(null, 2, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2)));

				result.Succeeded.Should().BeTrue();
				result.StocksProcessed.Should().Be(2);
				result.PriceRowsProcessed.Should().Be(2);
				result.FinancialReportsProcessed.Should().Be(2);
				result.Errors.Should().BeEmpty();
				marketProvider.BatchStockCodes.Should().HaveCount(2);
				financialProvider.BatchStockCodes.Should().HaveCount(2);
				indicatorRepository.BatchStockCodes.Should().HaveCount(2);
		}

		private sealed class FakeMarketProvider : IMarketDataProvider
		{
				public IReadOnlyCollection<string> BatchStockCodes { get; private set; } = [];
				public Task<IReadOnlyList<StockImportDto>> GetStocksAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<StockImportDto>>([new("600519", "Test", "SSE", "Stock", "Main", null, true, false), new("000001", "Test 2", "SZSE", "Stock", "Main", null, true, false)]);
				public Task<IReadOnlyList<DailyPriceImportDto>> GetDailyPricesAsync(string stockCode, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<DailyPriceImportDto>>([new(stockCode, startDate, 1, 2, 0.5m, 1.5m, 1.2m, 0.3m, 25, 100, 150, 1, null, null, false, "Test")]);
				public async Task<IReadOnlyList<DailyPriceImportDto>> GetDailyPricesAsync(IReadOnlyCollection<string> stockCodes, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) { BatchStockCodes = stockCodes; return (await Task.WhenAll(stockCodes.Select(stockCode => GetDailyPricesAsync(stockCode, startDate, endDate, cancellationToken)))).SelectMany(values => values).ToArray(); }
		}

		private sealed class FakeFinancialProvider : IFinancialDataProvider
		{
				public IReadOnlyCollection<string> BatchStockCodes { get; private set; } = [];
				public Task<IReadOnlyList<FinancialReportImportDto>> GetReportsAsync(string stockCode, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<FinancialReportImportDto>>([new(stockCode, new DateOnly(2025, 12, 31), "Annual", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 1), "Test", 100, 60, 40, 20, 15, 1, 200, 80, 120, 100, 50, 20, 5)]);
				public async Task<IReadOnlyList<FinancialReportImportDto>> GetReportsAsync(IReadOnlyCollection<string> stockCodes, CancellationToken cancellationToken = default) { BatchStockCodes = stockCodes; return (await Task.WhenAll(stockCodes.Select(stockCode => GetReportsAsync(stockCode, cancellationToken)))).SelectMany(values => values).ToArray(); }
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
				public IReadOnlyCollection<string> BatchStockCodes { get; private set; } = [];
				public Task<SyncResult> CalculateAndUpsertAsync(IReadOnlyCollection<string> stockCodes, CancellationToken cancellationToken = default) { BatchStockCodes = stockCodes; return Task.FromResult(new SyncResult("FinancialIndicator", stockCodes.Count, stockCodes.Count, 0, 0)); }
		}

		private sealed class FakeLogRepository : IImportLogRepository
		{
				public Task<ImportLogHandle> StartAsync(string dataType, int? dataSourceId = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImportLogHandle(1, dataType));
				public Task CompleteAsync(ImportLogHandle handle, SyncResult result, CancellationToken cancellationToken = default) => Task.CompletedTask;
		}
}
