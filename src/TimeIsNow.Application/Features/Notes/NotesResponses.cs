namespace TimeIsNow.Application.Features.Notes;

public sealed record NoteDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public required string Title { get; init; }
    public string Content { get; init; } = string.Empty;
    public bool IsPinned { get; init; }
    public List<string> Tags { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
