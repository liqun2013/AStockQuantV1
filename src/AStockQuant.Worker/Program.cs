using AStockQuant.Application.Services;
using AStockQuant.Application.Interfaces;
using AStockQuant.Infrastructure;
using AStockQuant.Worker.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<StockAnalysisService>();
builder.Services.AddScoped<IDataSyncService, DataSyncService>();
builder.Services.AddHostedService<StockScoreCalculateJob>();
builder.Services.AddHostedService<DataSyncHostedJob>();
await builder.Build().RunAsync();