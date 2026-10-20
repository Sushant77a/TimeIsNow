namespace TimeIsNow.Application.Features.DailyEntries;

public sealed record DailyEntryDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public DateOnly Date { get; init; }
    public required string Content { get; init; }
    public int? DurationMinutes { get; init; }
    public byte? Mood { get; init; }
    public byte? Energy { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
