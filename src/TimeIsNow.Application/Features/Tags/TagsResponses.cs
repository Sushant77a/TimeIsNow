namespace TimeIsNow.Application.Features.Tags;

public sealed record TagDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public DateTime CreatedAt { get; init; }
}
