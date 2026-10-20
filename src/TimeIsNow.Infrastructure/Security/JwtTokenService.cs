using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TimeIsNow.Application.Interfaces.Security;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Infrastructure.Security;

public sealed class JwtTokenService(JwtOptions options) : ITokenService
{
    private readonly SymmetricSecurityKey _signingKey = new(Encoding.UTF8.GetBytes(options.SecretKey));

    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(options.RefreshTokenDays);

    public AccessToken CreateAccessToken(User user) =>
        CreateAccessToken(user, TimeSpan.FromMinutes(options.AccessTokenMinutes));

    public AccessToken CreateAccessToken(User user, TimeSpan lifetime)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.Add(lifetime);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public string CreateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public string HashRefreshToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
