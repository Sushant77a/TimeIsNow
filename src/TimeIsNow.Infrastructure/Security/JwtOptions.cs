namespace TimeIsNow.Infrastructure.Security;

using Microsoft.Extensions.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "TimeIsNow";
    public string Audience { get; init; } = "TimeIsNow";
    public string SecretKey { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 7;

    public static JwtOptions FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        var secretKey = section["SecretKey"];

        if (string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException(
                "JWT secret key not found. " +
                "Set it with: dotnet user-secrets set \"Jwt:SecretKey\" \"<value>\"");

        return new JwtOptions
        {
            Issuer = section["Issuer"] ?? "TimeIsNow",
            Audience = section["Audience"] ?? "TimeIsNow",
            SecretKey = secretKey,
            AccessTokenMinutes = ParseInt(section["AccessTokenMinutes"], 15),
            RefreshTokenDays = ParseInt(section["RefreshTokenDays"], 7),
        };
    }

    private static int ParseInt(string? value, int fallback) =>
        int.TryParse(value, out var parsed) ? parsed : fallback;
}
