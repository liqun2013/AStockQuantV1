using AStockQuant.Application.Services;
using AStockQuant.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddScoped<StockAnalysisService>();
builder.Services.AddInfrastructure(builder.Configuration);
var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();
app.MapGet("/api/v1/health", () => new { success = true, message = "", data = new { status = "Healthy", time = DateTimeOffset.UtcNow } });
app.Run();
public partial class Program { }