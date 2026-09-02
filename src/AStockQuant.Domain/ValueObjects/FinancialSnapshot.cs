namespace AStockQuant.Domain.ValueObjects;

public sealed record FinancialSnapshot(
    string StockCode,
    DateOnly AsOfDate,
    decimal Roe,
    decimal Roic,
    decimal GrossMargin,
    decimal NetMargin,
    decimal OperatingCashFlowToNetProfit,
    decimal DebtAssetRatio,
    decimal CurrentRatio,
    decimal Pe,
    decimal Pb,
    decimal Eps,
    decimal Bvps,
    decimal MarketPrice,
    decimal RevenueGrowth3Y,
    decimal ProfitGrowth3Y,
    decimal ResearchExpenseRatio);