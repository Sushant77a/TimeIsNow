namespace TimeIsNow.Infrastructure.Ai;

using Microsoft.Extensions.Configuration;

public enum AiProtocol
{
    OpenAi,
    Anthropic,
}

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string Provider { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public int MaxTokens { get; init; }

    public static AiOptions FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);

        return new AiOptions
        {
            Provider = section["Provider"] ?? string.Empty,
            ApiKey = section["ApiKey"] ?? string.Empty,
            Model = section["Model"] ?? string.Empty,
            BaseUrl = section["BaseUrl"] ?? string.Empty,
            MaxTokens = int.TryParse(section["MaxTokens"], out var maxTokens) ? maxTokens : 0,
        };
    }

    public AiProtocol Protocol =>
        Provider.Contains("anthropic", StringComparison.OrdinalIgnoreCase)
        || Provider.Contains("claude", StringComparison.OrdinalIgnoreCase)
            ? AiProtocol.Anthropic
            : AiProtocol.OpenAi;

    public string? ResolveBaseUrl()
    {
        if (!string.IsNullOrWhiteSpace(BaseUrl))
            return BaseUrl.Trim().TrimEnd('/');

        return DefaultBaseUrls.TryGetValue(Provider.Trim(), out var url) ? url : null;
    }

    public string? ResolveModel()
    {
        if (!string.IsNullOrWhiteSpace(Model))
            return Model.Trim();

        return DefaultModels.TryGetValue(Provider.Trim(), out var model) && !string.IsNullOrEmpty(model)
            ? model
            : null;
    }

    private static readonly Dictionary<string, string> DefaultBaseUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        ["anthropic"] = "https://api.anthropic.com",
        ["claude"] = "https://api.anthropic.com",
        ["openai"] = "https://api.openai.com/v1",
        ["chatgpt"] = "https://api.openai.com/v1",
        ["gemini"] = "https://generativelanguage.googleapis.com/v1beta/openai",
        ["google"] = "https://generativelanguage.googleapis.com/v1beta/openai",
        ["openrouter"] = "https://openrouter.ai/api/v1",
        ["groq"] = "https://api.groq.com/openai/v1",
        ["deepseek"] = "https://api.deepseek.com/v1",
        ["mistral"] = "https://api.mistral.ai/v1",
        ["ollama"] = "http://localhost:11434/v1",
        ["lmstudio"] = "http://localhost:1234/v1",
    };

    private static readonly Dictionary<string, string> DefaultModels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["anthropic"] = "claude-sonnet-4-5",
        ["claude"] = "claude-sonnet-4-5",
        ["openai"] = "gpt-4o-mini",
        ["chatgpt"] = "gpt-4o-mini",
        ["gemini"] = "gemini-3.8-flash",
        ["google"] = "gemini-3.8-flash",
        ["openrouter"] = "openai/gpt-4o-mini",
        ["groq"] = "llama-3.3-70b-versatile",
        ["deepseek"] = "deepseek-chat",
        ["mistral"] = "mistral-large-latest",
    };
}
