using System.ComponentModel.DataAnnotations;

namespace TimeIsNow.Application.Features.Profile;

public sealed record ProfileRequest
{
    private string _displayName = null!;
    private string _currency = null!;

    [Required]
    [MaxLength(100)]
    public required string DisplayName
    {
        get => _displayName;
        init => _displayName = value?.Trim()!;
    }

    [Required]
    [MaxLength(10)]
    public required string Currency
    {
        get => _currency;
        init => _currency = value?.Trim()!;
    }

    [Range(0, 6)]
    public byte WeekStartsOn { get; init; }

    [MaxLength(20)]
    public string Theme { get; init; } = "system";
}
