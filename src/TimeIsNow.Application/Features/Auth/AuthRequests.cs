using System.ComponentModel.DataAnnotations;

namespace TimeIsNow.Application.Features.Auth;

public sealed record RegisterRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public required string Email { get; init; }

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public required string Password { get; init; }

    [Required]
    [MaxLength(100)]
    public required string DisplayName { get; init; }
}

public sealed record LoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public required string Email { get; init; }

    [Required]
    [MaxLength(100)]
    public required string Password { get; init; }
}
