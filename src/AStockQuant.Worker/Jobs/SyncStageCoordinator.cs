namespace AStockQuant.Worker.Jobs;

public sealed class SyncStageCoordinator : ISyncStageCoordinator
{
		private readonly object syncRoot = new();
		private readonly IReadOnlyDictionary<SyncStage, SemaphoreSlim> signals = Enum
			.GetValues<SyncStage>()
			.ToDictionary(stage => stage, _ => new SemaphoreSlim(0, 1));

		public Task WaitForCompletionAsync(SyncStage stage, CancellationToken cancellationToken) =>
			signals[stage].WaitAsync(cancellationToken);

		public void Complete(SyncStage stage)
		{
				lock (syncRoot)
				{
					var signal = signals[stage];
					if (signal.CurrentCount == 0)
						signal.Release();
				}
		}
}
