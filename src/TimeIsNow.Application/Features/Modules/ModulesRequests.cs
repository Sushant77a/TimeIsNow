using System.ComponentModel.DataAnnotations;

namespace TimeIsNow.Application.Features.Modules;

public sealed record UpdateModulesRequest
{
    [Required]
    [MinLength(1)]
    public required string[] Modules { get; init; }
}
