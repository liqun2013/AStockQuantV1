using AStockQuant.Application.Services;
using AStockQuant.Infrastructure;
using AStockQuant.Worker.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<StockAnalysisService>();
builder.Services.AddHostedService<StockScoreCalculateJob>();
await builder.Build().RunAsync();