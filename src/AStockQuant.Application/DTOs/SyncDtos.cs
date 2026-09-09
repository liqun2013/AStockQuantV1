namespace AStockQuant.Application.DTOs;

public sealed record StockImportDto(
		string StockCode,
		string StockName,
		string ExchangeCode,
		string SecurityType,
		string? MarketType,
		DateOnly? ListingDate,
		bool IsActive,
		bool IsST);

public sealed record DailyPriceImportDto(
		string StockCode,
		DateOnly TradeDate,
		decimal? OpenPrice,
		decimal? HighPrice,
		decimal? LowPrice,
		decimal? ClosePrice,
		decimal? PrevClosePrice,
		decimal? ChangeAmount,
		decimal? ChangePercent,
		long? Volume,
		decimal? Amount,
		decimal? TurnoverRate,
		decimal? TotalMarketCap,
		decimal? FloatMarketCap,
		bool IsSuspended,
		string Source);

public sealed record FinancialReportImportDto(
		string StockCode,
		DateOnly ReportPeriod,
		string ReportType,
		DateOnly? PublishDate,
		DateOnly? AnnouncementDate,
		string Source,
		decimal? Revenue,
		decimal? OperatingCost,
		decimal? GrossProfit,
		decimal? OperatingProfit,
		decimal? NetProfit,
		decimal? EarningsPerShare,
		decimal? TotalAssets,
		decimal? TotalLiabilities,
		decimal? TotalEquity,
		decimal? CurrentAssets,
		decimal? CurrentLiabilities,
		decimal? OperatingCashFlow,
		decimal? CapitalExpenditure);

public sealed record SyncResult(string DataType, int Requested, int Succeeded, int Skipped, int Failed, string? Error = null);
