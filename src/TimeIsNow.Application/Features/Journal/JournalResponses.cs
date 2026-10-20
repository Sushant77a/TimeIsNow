namespace TimeIsNow.Application.Features.Journal;

public sealed record JournalEntryDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public DateOnly Date { get; init; }
    public string? Title { get; init; }
    public required string Content { get; init; }
    public byte? Mood { get; init; }
    public byte? Energy { get; init; }
    public List<string> Tags { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
