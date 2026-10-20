using System.Text.Json.Serialization;

namespace TimeIsNow.Application.Common.ResponseWrappers;

public sealed class ApiResponse<T>(string message, string status, int statusCode, T? data)
{
    [JsonPropertyName("message")]
    public string Message { get; } = message;

    [JsonPropertyName("status")]
    public string Status { get; } = status;

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; } = statusCode;

    [JsonPropertyName("data")]
    public T? Data { get; } = data;
}

public sealed class ApiResponseWithoutData(string message, string status, int statusCode)
{
    [JsonPropertyName("message")]
    public string Message { get; } = message;

    [JsonPropertyName("status")]
    public string Status { get; } = status;

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; } = statusCode;
}

public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(T? data, string message, int statusCode = 200) =>
        new(message, "Success", statusCode, data);

    public static ApiResponse<object?> Fail(string message, int statusCode = 400, object? data = null) =>
        new(message, "Error", statusCode, data);

    public static ApiResponseWithoutData Success(string message, int statusCode = 200) =>
        new(message, "Success", statusCode);
}
