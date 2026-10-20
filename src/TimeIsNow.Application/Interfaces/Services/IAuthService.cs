using TimeIsNow.Application.Features.Auth;

namespace TimeIsNow.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<UserDto> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<McpTokenResponse> CreateMcpTokenAsync(CancellationToken cancellationToken = default);
}
