namespace AStockQuant.Domain.Services;

internal static class ScoreMath
{
    public static decimal Clamp(decimal value) => Math.Min(100m, Math.Max(0m, value));
    public static decimal Scale(decimal value, decimal excellent, decimal maximum = 100m)
    {
        if (excellent <= 0m) return 0m;
        return Clamp(value / excellent * maximum);
    }
    public static string Grade(decimal score) => score switch
    {
        >= 90m => "Excellent",
        >= 75m => "Good",
        >= 60m => "Fair",
        _ => "Weak"
    };
}