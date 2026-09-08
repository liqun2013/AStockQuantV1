using AStockQuant.Application.Services;
using AStockQuant.Infrastructure;
using AStockQuant.Worker.Jobs;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<StockAnalysisService>();
builder.Services.AddSingleton<ISyncStageCoordinator, SyncStageCoordinator>();
builder.Services.AddHostedService<MarketDataSyncJob>();
builder.Services.AddHostedService<FinancialDataSyncJob>();
builder.Services.AddHostedService<IndicatorCalculateJob>();
builder.Services.AddHostedService<StockScoreCalculateJob>();
await builder.Build().RunAsync();