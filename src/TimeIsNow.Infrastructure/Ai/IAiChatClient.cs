using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimeIsNow.Infrastructure.Ai;

public sealed record AiChatRequest(string SystemPrompt, string UserPrompt, int? MaxTokens = null);

public interface IAiChatClient
{
    Task<string?> CompleteAsync(AiChatRequest request, CancellationToken cancellationToken = default);
}

internal static class AiJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}

internal static class AiHttp
{
    public static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"AI request failed with {(int)response.StatusCode} ({response.ReasonPhrase}): {body}");
    }
}
