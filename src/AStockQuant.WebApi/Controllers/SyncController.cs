using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AStockQuant.WebApi.Controllers;

[ApiController]
[Route("api/v1/sync")]
public sealed class SyncController(IDataSyncService syncService) : ControllerBase
{
		[HttpPost("stocks")]
		public Task<ActionResult<ApiResponse<SyncExecutionResult>>> SynchronizeStocks([FromBody] SyncRequest request, CancellationToken cancellationToken) => Synchronize(request, cancellationToken);

		[HttpPost("stocks/{code}")]
		public Task<ActionResult<ApiResponse<SyncExecutionResult>>> SynchronizeStock(string code, [FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, CancellationToken cancellationToken) =>
				Synchronize(new SyncRequest(code, 1, startDate, endDate), cancellationToken);

		private async Task<ActionResult<ApiResponse<SyncExecutionResult>>> Synchronize(SyncRequest request, CancellationToken cancellationToken)
		{
				if (request.MaxStocks is < 1 or > 20)
						return BadRequest(ApiResponse<SyncExecutionResult>.Fail("MaxStocks must be between 1 and 20."));
				if (string.IsNullOrWhiteSpace(request.StockCode) && request.MaxStocks == 1)
						return BadRequest(ApiResponse<SyncExecutionResult>.Fail("StockCode is required when MaxStocks is 1."));
				if (request.StartDate > request.EndDate)
						return BadRequest(ApiResponse<SyncExecutionResult>.Fail("StartDate must not be later than EndDate."));

				var result = await syncService.SynchronizeAsync(request, cancellationToken);
				return result.Succeeded
						? ApiResponse<SyncExecutionResult>.Ok(result)
						: StatusCode(StatusCodes.Status502BadGateway, ApiResponse<SyncExecutionResult>.Fail(string.Join("; ", result.Errors)));
		}
}
