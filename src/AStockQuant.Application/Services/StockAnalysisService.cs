using AStockQuant.Application.DTOs;
using AStockQuant.Application.Interfaces;
using AStockQuant.Domain.Services;
using AStockQuant.Domain.ValueObjects;

namespace AStockQuant.Application.Services;

public sealed class StockAnalysisService(IStockRepository repository, IScoreModelRuleRepository ruleRepository)
{
    private readonly RuleBasedScoreCalculator calculator = new();
    private readonly CompositeScoreCalculator compositeCalculator = new();

    public Task<PagedResult<StockDto>> GetStocksAsync(int pageIndex, int pageSize, string? market = null, string? industry = null, CancellationToken cancellationToken = default)
    {
        pageIndex = Math.Max(1, pageIndex);
        pageSize = Math.Clamp(pageSize, 1, 200);
        return repository.GetStocksAsync(pageIndex, pageSize, market, industry, cancellationToken);
    }

    public Task<StockDto?> GetStockAsync(string code, CancellationToken cancellationToken = default) => repository.GetStockAsync(code, cancellationToken);
    public Task<IReadOnlyList<DailyPriceDto>> GetDailyPricesAsync(string code, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken = default) => repository.GetDailyPricesAsync(code, startDate, endDate, cancellationToken);
    public Task<IReadOnlyList<InvestmentScoreDto>> GetRankingAsync(DateOnly scoreDate, decimal? minScore, CancellationToken cancellationToken = default) => repository.GetRankingAsync(scoreDate, minScore, cancellationToken);
    public Task<InvestmentScoreDto?> GetLatestScoreAsync(string code, CancellationToken cancellationToken = default) => repository.GetLatestScoreAsync(code, cancellationToken);
    public Task<ScoreExplanationDto?> GetLatestScoreExplanationAsync(string code, CancellationToken cancellationToken = default) => repository.GetLatestScoreExplanationAsync(code, cancellationToken);
    public Task<FinancialSnapshotDto?> GetFinancialSnapshotAsync(string code, DateOnly asOfDate, CancellationToken cancellationToken = default) => repository.GetFinancialSnapshotAsync(code, asOfDate, cancellationToken);

    public async Task<InvestmentScoreDto?> CalculateAndSaveScoreAsync(string code, DateOnly asOfDate, CancellationToken cancellationToken = default)
    {
        var snapshot = await repository.GetFinancialSnapshotAsync(code, asOfDate, cancellationToken);
        if (snapshot is null) return null;
        var weights = await GetWeightsAsync(cancellationToken);
        var buffettRules = await ruleRepository.GetIndicatorRulesAsync("BUFFETT", cancellationToken);
        var grahamRules = await ruleRepository.GetIndicatorRulesAsync("GRAHAM", cancellationToken);
        var fisherRules = await ruleRepository.GetIndicatorRulesAsync("FISHER", cancellationToken);
        var score = compositeCalculator.Calculate(
            snapshot.StockCode,
            snapshot.AsOfDate,
            calculator.Calculate(GetIndicatorValues("BUFFETT", snapshot), buffettRules),
            calculator.Calculate(GetIndicatorValues("GRAHAM", snapshot), grahamRules),
            calculator.Calculate(GetIndicatorValues("FISHER", snapshot), fisherRules),
            weights);
        var dto = new InvestmentScoreDto(score.StockCode, score.ScoreDate, score.BuffettScore, score.GrahamScore, score.FisherScore, score.FinalScore);
        await repository.SaveInvestmentScoreAsync(dto, ModelCode,
        [
            ToExplanation("BUFFETT", score.Buffett),
            ToExplanation("GRAHAM", score.Graham),
            ToExplanation("FISHER", score.Fisher)
        ], cancellationToken);
        return dto;
    }

    private static IReadOnlyDictionary<string, decimal?> GetIndicatorValues(string modelCode, FinancialSnapshotDto snapshot) => modelCode switch
    {
        "BUFFETT" => new Dictionary<string, decimal?>
        {
            ["B01"] = snapshot.LongTermRoe,
            ["B02"] = snapshot.FreeCashFlowMargin,
            ["B03"] = snapshot.ProfitabilityStability5Y,
            ["B04"] = snapshot.DebtAssetRatio
        },
        "GRAHAM" => new Dictionary<string, decimal?>
        {
            ["G01"] = snapshot.Pe,
            ["G02"] = snapshot.Pb,
            ["G03"] = snapshot.DividendYield,
            ["G04"] = snapshot.DebtAssetRatio,
            ["G05"] = snapshot.ProfitabilityStability5Y
        },
        "FISHER" => new Dictionary<string, decimal?>
        {
            ["F01"] = snapshot.RevenueGrowth3Y,
            ["F02"] = snapshot.ProfitGrowth3Y,
            ["F03"] = snapshot.LongTermRoe
        },
        _ => throw new ArgumentOutOfRangeException(nameof(modelCode), modelCode, "Unknown investment model code.")
    };

    private const string ModelCode = "VALUE_INVESTMENT";

    private static ScoreModelExplanationDto ToExplanation(string modelCode, ScoreResult result) => new(
        modelCode,
        result.Score,
        result.Components
            .OrderBy(component => component.Key, StringComparer.Ordinal)
            .Select(component => new ScoreComponentDto(component.Key, component.Value))
            .ToArray());

    private async Task<CompositeScoreWeights> GetWeightsAsync(CancellationToken cancellationToken)
    {
        var configuredWeights = await ruleRepository.GetModelWeightsAsync(ModelCode, cancellationToken);
        var weights = configuredWeights
            .GroupBy(item => item.ComponentCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Single().Weight, StringComparer.OrdinalIgnoreCase);

        if (!weights.TryGetValue("BUFFETT", out var buffett) ||
            !weights.TryGetValue("GRAHAM", out var graham) ||
            !weights.TryGetValue("FISHER", out var fisher) ||
            weights.Count != 3)
        {
            throw new InvalidOperationException($"Active score model '{ModelCode}' must define exactly BUFFETT, GRAHAM and FISHER weights.");
        }

        return new CompositeScoreWeights(buffett, graham, fisher);
    }
}