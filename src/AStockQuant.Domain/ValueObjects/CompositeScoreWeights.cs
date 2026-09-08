namespace AStockQuant.Domain.ValueObjects;

public sealed record CompositeScoreWeights
{
		public CompositeScoreWeights(decimal buffett, decimal graham, decimal fisher)
		{
				if (buffett < 0m || graham < 0m || fisher < 0m)
						throw new ArgumentOutOfRangeException(nameof(buffett), "Score weights cannot be negative.");

				var total = buffett + graham + fisher;
				if (total != 1m)
						throw new ArgumentException($"Score weights must sum to 1. Actual total: {total}.");

				Buffett = buffett;
				Graham = graham;
				Fisher = fisher;
		}

		public decimal Buffett { get; }
		public decimal Graham { get; }
		public decimal Fisher { get; }
}
