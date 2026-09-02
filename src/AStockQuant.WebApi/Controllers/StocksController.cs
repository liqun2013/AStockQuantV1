using AStockQuant.Application.DTOs;
using AStockQuant.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AStockQuant.WebApi.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class StocksController(StockAnalysisService service) : ControllerBase
{
    [HttpGet("stocks")]
    public async Task<ApiResponse<PagedResult<StockDto>>> GetStocks([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, [FromQuery] string? market = null, [FromQuery] string? industry = null, CancellationToken cancellationToken = default) => ApiResponse<PagedResult<StockDto>>.Ok(await service.GetStocksAsync(pageIndex, pageSize, market, industry, cancellationToken));

    [HttpGet("stocks/{code}")]
    public async Task<ActionResult<ApiResponse<StockDto>>> GetStock(string code, CancellationToken cancellationToken)
    {
        var stock = await service.GetStockAsync(code, cancellationToken);
        return stock is null ? NotFound(ApiResponse<StockDto>.Fail("Stock was not found.")) : ApiResponse<StockDto>.Ok(stock);
    }

    [HttpGet("stocks/{code}/daily")]
    public async Task<ApiResponse<IReadOnlyList<DailyPriceDto>>> GetDaily(string code, [FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, CancellationToken cancellationToken) => ApiResponse<IReadOnlyList<DailyPriceDto>>.Ok(await service.GetDailyPricesAsync(code, startDate, endDate, cancellationToken));

    [HttpGet("stocks/{code}/score")]
    public async Task<ActionResult<ApiResponse<InvestmentScoreDto>>> GetScore(string code, CancellationToken cancellationToken)
    {
        var score = await service.GetLatestScoreAsync(code, cancellationToken);
        return score is null ? NotFound(ApiResponse<InvestmentScoreDto>.Fail("Score was not found.")) : ApiResponse<InvestmentScoreDto>.Ok(score);
    }

    [HttpGet("stocks/{code}/financial")]
    public async Task<ActionResult<ApiResponse<FinancialSnapshotDto>>> GetFinancial(string code, [FromQuery] DateOnly? asOfDate, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code)) return BadRequest(ApiResponse<FinancialSnapshotDto>.Fail("Stock code is required."));
        var snapshot = await service.GetFinancialSnapshotAsync(code, asOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        return snapshot is null ? NotFound(ApiResponse<FinancialSnapshotDto>.Fail("Financial snapshot was not found.")) : ApiResponse<FinancialSnapshotDto>.Ok(snapshot);
    }

    [HttpPost("stocks/{code}/indicators/calculate")]
    public async Task<ActionResult<ApiResponse<InvestmentScoreDto>>> Calculate(string code, [FromQuery] DateOnly? asOfDate, CancellationToken cancellationToken)
    {
        var score = await service.CalculateAndSaveScoreAsync(code, asOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        return score is null ? NotFound(ApiResponse<InvestmentScoreDto>.Fail("Financial snapshot was not found.")) : ApiResponse<InvestmentScoreDto>.Ok(score);
    }

    [HttpGet("ranking")]
    public async Task<ApiResponse<IReadOnlyList<InvestmentScoreDto>>> GetRanking([FromQuery] DateOnly? scoreDate, [FromQuery] decimal? minScore, CancellationToken cancellationToken) => ApiResponse<IReadOnlyList<InvestmentScoreDto>>.Ok(await service.GetRankingAsync(scoreDate ?? DateOnly.FromDateTime(DateTime.UtcNow), minScore, cancellationToken));
}