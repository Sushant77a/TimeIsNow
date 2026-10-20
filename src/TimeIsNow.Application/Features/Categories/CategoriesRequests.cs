using System.ComponentModel.DataAnnotations;

namespace TimeIsNow.Application.Features.Categories;

public sealed record CategoryRequest
{
    private string _name = null!;
    private string _color = null!;

    [Required]
    [MaxLength(100)]
    public required string Name
    {
        get => _name;
        init => _name = value?.Trim()!;
    }

    [MaxLength(500)]
    public string? Description { get; init; }

    [Required]
    [MaxLength(7)]
    public required string Color
    {
        get => _color;
        init => _color = value?.Trim()!;
    }

    [MaxLength(50)]
    public string? Icon { get; init; }

    public int SortOrder { get; init; }

    public bool IsArchived { get; init; }
}
