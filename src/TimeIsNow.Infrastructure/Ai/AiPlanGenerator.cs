using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TimeIsNow.Application.Features.Practices;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Application.Services;

namespace TimeIsNow.Infrastructure.Ai;

public sealed class AiPlanGenerator(
    AiOptions options,
    IAiChatClient client,
    TemplatePlanGenerator template,
    ILogger<AiPlanGenerator> logger) : IPracticePlanGenerator
{
    private const int PlanMaxTokens = 3000;
    private const int MaxSessions = 6;

    private const string SystemPrompt =
        "You design short daily practice plans for a personal life-tracking app. " +
        "The user describes a practice, for example chakra balancing, meditation, stretching or language drills. " +
        "Answer with ONLY a JSON object and nothing else: no markdown, no code fences, no commentary. " +
        "The object must match exactly this shape: " +
        "{\"title\": string, \"overview\": string, \"sessionsPerDay\": number, \"durationDays\": number, " +
        "\"sessions\": [{\"order\": number, \"name\": string, \"timeOfDay\": string, \"durationMinutes\": number, \"instructions\": string}]}. " +
        "Rules: sessionsPerDay between 1 and 6; durationDays between 7 and 90; " +
        "sessions is an array of exactly sessionsPerDay entries; " +
        "each session has a short name, timeOfDay of Morning, Midday, Afternoon, Evening, Night or Anytime, " +
        "durationMinutes between 5 and 30, and instructions written as 2 to 4 concrete sentences the user can follow offline. " +
        "Overview is 2 to 4 sentences describing what the practice is and how to approach the plan. " +
        "Keep every string plain text without line breaks.";

    public async Task<PracticePlanDraftDto> GenerateAsync(
        PracticePlanRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var text = await client.CompleteAsync(
                new AiChatRequest(SystemPrompt, BuildPrompt(request), PlanMaxTokens),
                cancellationToken);

            var draft = Parse(text, request);
            if (draft is not null)
                return draft;

            logger.LogWarning(
                "AI plan generation for provider '{Provider}' returned an unusable plan. Falling back to the template plan.",
                options.Provider);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "AI plan generation for provider '{Provider}' failed. Falling back to the template plan.",
                options.Provider);
        }

        return await template.GenerateAsync(request, cancellationToken);
    }

    private static string BuildPrompt(PracticePlanRequest request)
    {
        var prompt = request.Prompt.Trim();
        var sessionsPerDay = Math.Clamp(request.SessionsPerDay ?? 3, 1, MaxSessions);
        var days = Math.Clamp(request.Days ?? 21, 1, 365);

        return
            $"Practice: {prompt}\n" +
            $"Sessions per day: {sessionsPerDay}\n" +
            $"Plan length in days: {days}";
    }

    private PracticePlanDraftDto? Parse(string? text, PracticePlanRequest request)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var json = ExtractJsonObject(text);
        if (json is null)
            return null;

        PlanPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<PlanPayload>(json, ParseOptions);
        }
        catch (JsonException)
        {
            return null;
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.Title) || payload.Sessions is null)
            return null;

        var sessions = payload.Sessions
            .Where(session => !string.IsNullOrWhiteSpace(session.Name) && !string.IsNullOrWhiteSpace(session.Instructions))
            .Take(MaxSessions)
            .Select((session, index) => new PracticePlanSessionDto
            {
                Name = session.Name!.Trim(),
                TimeOfDay = NormalizeTimeOfDay(session.TimeOfDay),
                DurationMinutes = session.DurationMinutes is >= 1 and <= 600 ? session.DurationMinutes : 10,
                Instructions = session.Instructions!.Trim(),
            })
            .ToList();

        if (sessions.Count == 0)
            return null;

        return new PracticePlanDraftDto
        {
            Title = payload.Title.Trim(),
            Overview = string.IsNullOrWhiteSpace(payload.Overview) ? null : payload.Overview.Trim(),
            StartDate = request.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            SessionsPerDay = sessions.Count,
            DurationDays = Math.Clamp(payload.DurationDays ?? request.Days ?? 21, 1, 365),
            Sessions = sessions,
            Source = "ai",
            Provider = options.Provider,
        };
    }

    private static string? ExtractJsonObject(string text)
    {
        var cleaned = FenceRegex.Replace(text.Trim(), string.Empty).Trim();
        var start = cleaned.IndexOf('{');
        var end = cleaned.LastIndexOf('}');
        return start >= 0 && end > start ? cleaned[start..(end + 1)] : null;
    }

    private static string? NormalizeTimeOfDay(string? timeOfDay)
    {
        if (string.IsNullOrWhiteSpace(timeOfDay))
            return null;

        var normalized = timeOfDay.Trim();
        return normalized.Length <= 30 ? normalized : normalized[..30];
    }

    private static readonly Regex FenceRegex = new(
        @"^```(?:json)?\s*|\s*```$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly JsonSerializerOptions ParseOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private sealed class PlanPayload
    {
        public string? Title { get; set; }
        public string? Overview { get; set; }
        public int? SessionsPerDay { get; set; }
        public int? DurationDays { get; set; }
        public List<PlanSessionPayload>? Sessions { get; set; }
    }

    private sealed class PlanSessionPayload
    {
        public string? Name { get; set; }
        public string? TimeOfDay { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Instructions { get; set; }
    }
}
