namespace TimeIsNow.Application.Features.Profile;

public sealed record ProfileDto
{
    public int Id { get; init; }
    public required string DisplayName { get; init; }
    public required string Currency { get; init; }
    public byte WeekStartsOn { get; init; }
    public required string Theme { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
