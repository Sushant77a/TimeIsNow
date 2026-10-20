using System.Net.Http.Json;

namespace TimeIsNow.Infrastructure.Ai;

public sealed class AnthropicChatClient(AiOptions options) : IAiChatClient
{
    private const string AnthropicVersion = "2023-06-01";

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

        var payload = new MessagesRequest
        {
            Model = model,
            MaxTokens = options.MaxTokens > 0 ? options.MaxTokens : 1024,
            System = request.SystemPrompt,
            Messages = [new UserMessage { Role = "user", Content = request.UserPrompt }],
        };

        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl}/v1/messages")
        {
            Content = JsonContent.Create(payload, options: AiJson.Options),
        };
        httpRequest.Headers.Add("x-api-key", options.ApiKey);
        httpRequest.Headers.Add("anthropic-version", AnthropicVersion);

        using var response = await Http.SendAsync(httpRequest, cancellationToken);
        await AiHttp.EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<MessagesResponse>(
            AiJson.Options,
            cancellationToken);

        if (result?.Content is null)
            return null;

        return string.Concat(result.Content
            .Where(block => block.Type == "text" && block.Text is not null)
            .Select(block => block.Text));
    }

    private sealed class MessagesRequest
    {
        public string Model { get; init; } = string.Empty;
        public int MaxTokens { get; init; }
        public string System { get; init; } = string.Empty;
        public List<UserMessage> Messages { get; init; } = [];
    }

    private sealed class UserMessage
    {
        public string Role { get; init; } = "user";
        public string Content { get; init; } = string.Empty;
    }

    private sealed class MessagesResponse
    {
        public List<ContentBlock>? Content { get; init; }
    }

    private sealed class ContentBlock
    {
        public string Type { get; init; } = string.Empty;
        public string? Text { get; init; }
    }
}
