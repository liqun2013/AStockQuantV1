using AStockQuant.Application.Interfaces;
using AStockQuant.Application.Services;
using AStockQuant.Infrastructure.Persistence;
using AStockQuant.Infrastructure.Repositories;
using AStockQuant.Infrastructure.AKTools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace AStockQuant.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration? configuration = null)
    {
        var akToolsUrl = configuration?["AKTools:BaseUrl"] ?? "http://localhost:44802/";
        services.AddHttpClient<IAkToolsClient, AkToolsClient>(client => client.BaseAddress = new Uri(akToolsUrl));
        return services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>()
            .AddScoped<IStockRepository, StockRepository>()
            .AddScoped<IScreeningRepository, ScreeningRepository>()
            .AddScoped<IIndustryOverrideRepository, IndustryOverrideRepository>()
            .AddScoped<IIndustryOverrideService, IndustryOverrideService>()
            .AddScoped<IScoreModelRuleRepository, ScoreModelRuleRepository>()
            .AddScoped<IMarketDataRepository, MarketDataRepository>()
            .AddScoped<IMarketDataProvider, AkToolsMarketDataProvider>()
            .AddScoped<IFinancialDataRepository, FinancialDataRepository>()
            .AddScoped<IFinancialIndicatorRepository, FinancialIndicatorRepository>()
            .AddScoped<IImportLogRepository, ImportLogRepository>()
            .AddScoped<IFinancialDataProvider, AkToolsFinancialDataProvider>();
    }
}