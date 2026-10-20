using System.ComponentModel.DataAnnotations;

namespace TimeIsNow.Application.Features.Notes;

public sealed record NoteRequest
{
    private string _title = null!;

    [Required]
    public int CategoryId { get; init; }

    [Required]
    [MaxLength(300)]
    public required string Title
    {
        get => _title;
        init => _title = value?.Trim()!;
    }

    public string Content { get; init; } = string.Empty;

    public bool IsPinned { get; init; }

    public List<string>? Tags { get; init; }
}

public sealed record NoteListQuery
{
    public int? CategoryId { get; init; }
    public string? Search { get; init; }
    public string? Tag { get; init; }
    public bool? Pinned { get; init; }
}
