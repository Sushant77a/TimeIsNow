using TimeIsNow.Application.Features.Practices;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.Application.Services;

public sealed class TemplatePlanGenerator : IPracticePlanGenerator
{
    private const int DefaultSessionsPerDay = 3;
    private const int DefaultDurationDays = 21;
    private const int DefaultSessionMinutes = 10;

    private static readonly (string Name, string TimeOfDay, string Instructions)[] Slots =
    [
        (
            "Morning opening",
            "Morning",
            "Sit comfortably with a long spine and take five slow breaths. Spend the session on your practice at a gentle pace, working steadily from start to finish. Close by naming one intention to carry into the day."
        ),
        (
            "Midday reset",
            "Midday",
            "Pause whatever you are doing and soften your gaze. Take five breaths, then run through your practice slowly enough to feel each part of it. Finish with two minutes of quiet rest before returning to your day."
        ),
        (
            "Afternoon focus",
            "Afternoon",
            "Stand or sit and take three deep breaths to arrive. Repeat your practice with full attention, letting the rhythm of the breath set the pace. End by rolling the shoulders and releasing any leftover tension."
        ),
        (
            "Evening wind-down",
            "Evening",
            "Find a quiet, dim space. Breathe out longer than you breathe in and move through your practice at half speed. Lie down afterwards if that is comfortable and rest for a couple of minutes."
        ),
        (
            "Night release",
            "Night",
            "Do this one lying down. Take ten unhurried breaths, then move through your practice barely moving, letting each part dissolve into the last. Fall asleep to the rhythm of the breath if you can."
        ),
        (
            "Anytime top-up",
            "Anytime",
            "A short, flexible round: three breaths, one slow pass through your practice, then three more breaths. Use it whenever you have a spare few minutes and feel out of sync."
        ),
    ];

    public Task<PracticePlanDraftDto> GenerateAsync(
        PracticePlanRequest request, CancellationToken cancellationToken = default)
    {
        var prompt = request.Prompt.Trim();
        var sessionsPerDay = Math.Clamp(request.SessionsPerDay ?? DefaultSessionsPerDay, 1, Slots.Length);
        var durationDays = Math.Clamp(request.Days ?? DefaultDurationDays, 1, 365);
        var title = char.ToUpperInvariant(prompt[0]) + prompt[1..];

        var sessions = new List<PracticePlanSessionDto>();
        for (var i = 0; i < sessionsPerDay; i++)
        {
            var slot = Slots[i];
            sessions.Add(new PracticePlanSessionDto
            {
                Name = $"{title} - {slot.Name}",
                TimeOfDay = slot.TimeOfDay,
                DurationMinutes = DefaultSessionMinutes,
                Instructions = slot.Instructions,
            });
        }

        var draft = new PracticePlanDraftDto
        {
            Title = title,
            Overview =
                $"A {durationDays}-day plan with {sessionsPerDay} short session{(sessionsPerDay == 1 ? "" : "s")} a day, " +
                $"built around {prompt.ToLowerInvariant()}. " +
                "Do each session when its time of day comes around and mark it done afterwards. " +
                "Consistency matters more than intensity: a short session you actually finish beats a long one you skip.",
            StartDate = request.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            SessionsPerDay = sessionsPerDay,
            DurationDays = durationDays,
            Sessions = sessions,
            Source = "template",
        };

        return Task.FromResult(draft);
    }
}
