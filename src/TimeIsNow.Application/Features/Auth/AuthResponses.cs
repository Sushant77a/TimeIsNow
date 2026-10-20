namespace TimeIsNow.Application.Features.Auth;

public sealed record UserDto
{
    public int Id { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
}

public sealed record AuthResponse
{
    public required string AccessToken { get; init; }
    public DateTime AccessTokenExpiresAt { get; init; }
    public required UserDto User { get; init; }
}

public sealed record AuthResult(
    AuthResponse Response,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);

public sealed record McpTokenResponse(string Token, DateTime ExpiresAt);
