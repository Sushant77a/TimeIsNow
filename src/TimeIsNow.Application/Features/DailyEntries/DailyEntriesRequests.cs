using System.ComponentModel.DataAnnotations;

namespace TimeIsNow.Application.Features.DailyEntries;

public sealed record DailyEntryRequest
{
    private string _content = null!;

    [Required]
    public int CategoryId { get; init; }

    [Required]
    public DateOnly Date { get; init; }

    [Required]
    public required string Content
    {
        get => _content;
        init => _content = value?.Trim()!;
    }

    [Range(1, 1440)]
    public int? DurationMinutes { get; init; }

    [Range(1, 5)]
    public byte? Mood { get; init; }

    [Range(1, 5)]
    public byte? Energy { get; init; }
}

public sealed record DailyEntryListQuery
{
    public int? CategoryId { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}
