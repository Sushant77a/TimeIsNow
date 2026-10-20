using System.ComponentModel.DataAnnotations;

namespace TimeIsNow.Application.Features.Journal;

public sealed record JournalEntryRequest
{
    private string _content = null!;

    [Required]
    public int CategoryId { get; init; }

    [Required]
    public DateOnly Date { get; init; }

    [MaxLength(300)]
    public string? Title { get; init; }

    [Required]
    public required string Content
    {
        get => _content;
        init => _content = value?.Trim()!;
    }

    [Range(1, 5)]
    public byte? Mood { get; init; }

    [Range(1, 5)]
    public byte? Energy { get; init; }

    public List<string>? Tags { get; init; }
}

public sealed record JournalListQuery
{
    public int? CategoryId { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public string? Search { get; init; }
}
