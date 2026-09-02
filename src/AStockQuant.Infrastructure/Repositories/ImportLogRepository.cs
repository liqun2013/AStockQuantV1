using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Infrastructure.Persistence;
using Dapper;

namespace AStockQuant.Infrastructure.Repositories;

public sealed class ImportLogRepository(ISqlConnectionFactory connectionFactory) : IImportLogRepository
{
		public async Task<ImportLogHandle> StartAsync(string dataType, int? dataSourceId = null, CancellationToken cancellationToken = default)
		{
				var correlationId = Guid.NewGuid().ToString("N");
				using var connection = connectionFactory.CreateConnection();
				const string sql = """
INSERT INTO System.DataImportLog (DataSourceId, DataType, StartTime, Status, RequestCount, SuccessCount, FailedCount, InsertedCount, UpdatedCount, SkippedCount, CorrelationId, CreatedTime)
OUTPUT INSERTED.ImportLogId
VALUES (@DataSourceId, @DataType, SYSUTCDATETIME(), 'Running', 0, 0, 0, 0, 0, 0, @CorrelationId, SYSUTCDATETIME());
""";
				var id = await connection.ExecuteScalarAsync<long>(new CommandDefinition(sql, new { DataSourceId = dataSourceId, DataType = dataType, CorrelationId = correlationId }, cancellationToken: cancellationToken));
				return new ImportLogHandle(id, correlationId);
		}

		public async Task CompleteAsync(ImportLogHandle handle, SyncResult result, CancellationToken cancellationToken = default)
		{
				using var connection = connectionFactory.CreateConnection();
				const string sql = """
UPDATE System.DataImportLog
SET EndTime = SYSUTCDATETIME(),
				Status = CASE WHEN @Failed = 0 THEN 'SUCCESS' ELSE 'FAILED' END,
		RequestCount = @Requested,
		SuccessCount = @Succeeded,
		FailedCount = @Failed,
		SkippedCount = @Skipped,
		ErrorMessage = @Error,
		ErrorDetail = @Error,
		CorrelationId = @CorrelationId
WHERE ImportLogId = @ImportLogId;
""";
				await connection.ExecuteAsync(new CommandDefinition(sql, new
				{
						ImportLogId = handle.ImportLogId,
						handle.CorrelationId,
						Requested = result.Requested,
						Succeeded = result.Succeeded,
						Failed = result.Failed,
						Skipped = result.Skipped,
						Error = result.Error
				}, cancellationToken: cancellationToken));
		}
}
