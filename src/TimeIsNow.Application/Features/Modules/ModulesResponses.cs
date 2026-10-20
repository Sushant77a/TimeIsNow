namespace TimeIsNow.Application.Features.Modules;

public sealed record ModuleSetDto
{
    public required string[] Modules { get; init; }
}
