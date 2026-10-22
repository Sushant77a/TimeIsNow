using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TimeIsNow.Infrastructure.Ai;

public sealed class OpenAiChatClient(AiOptions options) : IAiChatClient
{
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(15),
    };

    public async Task<string?> CompleteAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = options.ResolveBaseUrl()
            ?? throw new InvalidOperationException(
                $"Ai:BaseUrl is required for provider '{options.Provider}'.");
        var model = options.ResolveModel()
            ?? throw new InvalidOperationException(
                $"Ai:Model is required for provider '{options.Provider}'.");

        var payload = new ChatRequest
        {
            Model = model,
            MaxTokens = request.MaxTokens ?? (options.MaxTokens > 0 ? options.MaxTokens : null),
            Messages =
            [
                new ChatMessage { Role = "system", Content = request.SystemPrompt },
                new ChatMessage { Role = "user", Content = request.UserPrompt },
            ],
        };

        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl}/chat/completions")
        {
            Content = JsonContent.Create(payload, options: AiJson.Options),
        };
        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", options.ApiKey);

        using var response = await Http.SendAsync(httpRequest, cancellationToken);
        await AiHttp.EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(
            AiJson.Options,
            cancellationToken);

        return result?.Choices?.FirstOrDefault()?.Message?.Content;
    }

    private sealed class ChatRequest
    {
        public string Model { get; init; } = string.Empty;
        public int? MaxTokens { get; init; }
        public List<ChatMessage> Messages { get; init; } = [];
    }

    private sealed class ChatMessage
    {
        public string Role { get; init; } = "user";
        public string Content { get; init; } = string.Empty;
    }

    private sealed class ChatResponse
    {
        public List<Choice>? Choices { get; init; }
    }

    private sealed class Choice
    {
        public ChatMessage? Message { get; init; }
    }
}
