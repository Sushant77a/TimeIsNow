namespace TimeIsNow.Application.Features.Categories;

public sealed record CategoryDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Color { get; init; }
    public string? Icon { get; init; }
    public int SortOrder { get; init; }
    public bool IsArchived { get; init; }
    public string[] Modules { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
