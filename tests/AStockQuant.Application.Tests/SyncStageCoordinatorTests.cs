using AStockQuant.Worker.Jobs;
using FluentAssertions;
using Xunit;

namespace AStockQuant.Application.Tests;

public sealed class SyncStageCoordinatorTests
{
		[Fact]
		public async Task WaitForCompletionAsync_BlocksUntilStageIsCompleted()
		{
				var coordinator = new SyncStageCoordinator();
				var waitTask = coordinator.WaitForCompletionAsync(SyncStage.MarketData, CancellationToken.None);

				waitTask.IsCompleted.Should().BeFalse();
				coordinator.Complete(SyncStage.MarketData);

				await waitTask;
		}

		[Fact]
		public async Task Complete_CoalescesUnconsumedSignals()
		{
				var coordinator = new SyncStageCoordinator();
				coordinator.Complete(SyncStage.FinancialData);
				coordinator.Complete(SyncStage.FinancialData);

				await coordinator.WaitForCompletionAsync(SyncStage.FinancialData, CancellationToken.None);
				var secondWait = coordinator.WaitForCompletionAsync(SyncStage.FinancialData, CancellationToken.None);

				(await Task.WhenAny(secondWait, Task.Delay(TimeSpan.FromMilliseconds(50)))).Should().NotBeSameAs(secondWait);
				coordinator.Complete(SyncStage.FinancialData);
				await secondWait;
		}
}
