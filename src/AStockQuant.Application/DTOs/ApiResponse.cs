namespace AStockQuant.Application.DTOs;
public sealed record ApiResponse<T>(bool Success, string Message, T? Data)
{
    public static ApiResponse<T> Ok(T data, string message = "") => new(true, message, data);
    public static ApiResponse<T> Fail(string message) => new(false, message, default);
}
public sealed record PagedResult<T>(int Total, IReadOnlyList<T> Items);