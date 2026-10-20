using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.Auth;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Security;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Services;

public sealed class AuthService(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IUserProfileRepository profiles,
    ICategoryRepository categories,
    ICategoryModuleRepository categoryModules,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IAuthService
{
    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);

        if (await users.EmailExistsAsync(email, cancellationToken))
            throw new Conflict409Exception(ApiMessages.EmailAlreadyRegistered);

        var now = DateTime.UtcNow;
        var user = new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            DisplayName = request.DisplayName.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        profiles.Add(new UserProfile
        {
            UserId = user.Id,
            DisplayName = user.DisplayName,
            Currency = "INR",
            WeekStartsOn = 1,
            Theme = "system",
            CreatedAt = now,
            UpdatedAt = now,
        });

        var createdCategories = CreateDefaultCategories(user.Id, now).ToList();
        foreach (var category in createdCategories)
            categories.Add(category);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        SeedDefaultModules(user.Id, createdCategories, now);

        var result = IssueTokens(user, now);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await users.FindByEmailAsync(email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new Unauthorized401Exception(ApiMessages.InvalidCredentials);

        var now = DateTime.UtcNow;
        var result = IssueTokens(user, now);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new Unauthorized401Exception(ApiMessages.InvalidRefreshToken);

        var now = DateTime.UtcNow;
        var hash = tokenService.HashRefreshToken(refreshToken);
        var stored = await refreshTokens.FindByHashAsync(hash, cancellationToken);

        if (stored is null)
            throw new Unauthorized401Exception(ApiMessages.InvalidRefreshToken);

        if (stored.RevokedAt is not null || stored.ExpiresAt <= now)
        {
            // Reuse of a rotated/expired token: revoke the whole family for safety.
            var active = await refreshTokens.GetActiveForUserAsync(stored.UserId, cancellationToken);
            refreshTokens.RemoveRange(active);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw new Unauthorized401Exception(ApiMessages.InvalidRefreshToken);
        }

        var user = await users.FindByIdAsync(stored.UserId, cancellationToken)
            ?? throw new Unauthorized401Exception(ApiMessages.InvalidRefreshToken);

        var result = IssueTokens(user, now);
        stored.RevokedAt = now;
        stored.ReplacedByToken = tokenService.HashRefreshToken(result.RefreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var hash = tokenService.HashRefreshToken(refreshToken);
        var stored = await refreshTokens.FindByHashAsync(hash, cancellationToken);

        if (stored is null)
            return;

        stored.RevokedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated)
            throw new Unauthorized401Exception(ApiMessages.NotAuthenticated);

        var user = await users.FindByIdAsync(currentUser.UserId, cancellationToken)
            ?? throw new Unauthorized401Exception(ApiMessages.NotAuthenticated);

        return ToDto(user);
    }

    public async Task<McpTokenResponse> CreateMcpTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated)
            throw new Unauthorized401Exception(ApiMessages.NotAuthenticated);

        var user = await users.FindByIdAsync(currentUser.UserId, cancellationToken)
            ?? throw new Unauthorized401Exception(ApiMessages.NotAuthenticated);

        var token = tokenService.CreateAccessToken(user, TimeSpan.FromDays(30));

        return new McpTokenResponse(token.Value, token.ExpiresAt);
    }

    private AuthResult IssueTokens(User user, DateTime now)
    {
        var accessToken = tokenService.CreateAccessToken(user);
        var refreshToken = tokenService.CreateRefreshToken();
        var refreshExpiresAt = now.Add(tokenService.RefreshTokenLifetime);

        refreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenService.HashRefreshToken(refreshToken),
            ExpiresAt = refreshExpiresAt,
            CreatedAt = now,
        });

        var response = new AuthResponse
        {
            AccessToken = accessToken.Value,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            User = ToDto(user),
        };

        return new AuthResult(response, refreshToken, refreshExpiresAt);
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName,
    };

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static IEnumerable<Category> CreateDefaultCategories(int userId, DateTime now) =>
    [
        new Category { UserId = userId, Name = "Personal", Color = "#4F46E5", Icon = "user", SortOrder = 1, CreatedAt = now, UpdatedAt = now },
        new Category { UserId = userId, Name = "Study", Color = "#059669", Icon = "book", SortOrder = 2, CreatedAt = now, UpdatedAt = now },
        new Category { UserId = userId, Name = "Work", Color = "#D97706", Icon = "briefcase", SortOrder = 3, CreatedAt = now, UpdatedAt = now },
        new Category { UserId = userId, Name = "Leisure", Color = "#DB2777", Icon = "gamepad", SortOrder = 4, CreatedAt = now, UpdatedAt = now },
    ];

    private void SeedDefaultModules(int userId, List<Category> createdCategories, DateTime now)
    {
        var globalKeys = new[]
        {
            Domain.Enums.ModuleKey.Dashboard,
            Domain.Enums.ModuleKey.Todos,
            Domain.Enums.ModuleKey.Habits,
            Domain.Enums.ModuleKey.Finance,
        };
        var order = 0;
        foreach (var key in globalKeys)
        {
            categoryModules.AddGlobal(new GlobalModule
            {
                UserId = userId,
                ModuleKey = key,
                SortOrder = order++,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        var personalKeys = new[]
        {
            Domain.Enums.ModuleKey.Dashboard,
            Domain.Enums.ModuleKey.DailyEntries,
            Domain.Enums.ModuleKey.Journal,
            Domain.Enums.ModuleKey.Schedule,
            Domain.Enums.ModuleKey.Goals,
            Domain.Enums.ModuleKey.Notes,
            Domain.Enums.ModuleKey.Settings,
        };
        var standardKeys = new[]
        {
            Domain.Enums.ModuleKey.Dashboard,
            Domain.Enums.ModuleKey.DailyEntries,
            Domain.Enums.ModuleKey.Schedule,
            Domain.Enums.ModuleKey.Goals,
            Domain.Enums.ModuleKey.Notes,
            Domain.Enums.ModuleKey.Settings,
        };

        foreach (var category in createdCategories)
        {
            var keys = category.Name.Equals("Personal", StringComparison.OrdinalIgnoreCase)
                ? personalKeys
                : standardKeys;
            var catOrder = 0;
            foreach (var key in keys)
            {
                categoryModules.AddCategory(new CategoryModule
                {
                    CategoryId = category.Id,
                    UserId = userId,
                    ModuleKey = key,
                    SortOrder = catOrder++,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
            }
        }
    }
}
