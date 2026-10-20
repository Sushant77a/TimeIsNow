using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Interfaces.Security;

public sealed record AccessToken(string Value, DateTime ExpiresAt);

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
    AccessToken CreateAccessToken(User user, TimeSpan lifetime);
    string CreateRefreshToken();
    string HashRefreshToken(string token);
    TimeSpan RefreshTokenLifetime { get; }
}
