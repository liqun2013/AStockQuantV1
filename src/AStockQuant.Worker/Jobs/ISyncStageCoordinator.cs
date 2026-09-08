namespace AStockQuant.Worker.Jobs;

public enum SyncStage
{
		MarketData,
		FinancialData,
		FinancialIndicator
}

public interface ISyncStageCoordinator
{
		Task WaitForCompletionAsync(SyncStage stage, CancellationToken cancellationToken);
		void Complete(SyncStage stage);
}
