using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Features.Auth;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    private const string RefreshCookieName = "tin_refresh";
    private const string RefreshCookiePath = "/api/auth";

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        WriteRefreshCookie(result);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse.Success(result.Response, ApiMessages.Registered, StatusCodes.Status201Created));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        WriteRefreshCookie(result);

        return Ok(ApiResponse.Success(result.Response, ApiMessages.LoggedIn));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var token = Request.Cookies[RefreshCookieName] ?? string.Empty;
        var result = await authService.RefreshAsync(token, cancellationToken);
        WriteRefreshCookie(result);

        return Ok(ApiResponse.Success(result.Response, ApiMessages.TokensRefreshed));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var token = Request.Cookies[RefreshCookieName] ?? string.Empty;
        await authService.LogoutAsync(token, cancellationToken);
        ClearRefreshCookie();

        return Ok(ApiResponse.Success(ApiMessages.LoggedOut));
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var user = await authService.GetCurrentAsync(cancellationToken);

        return Ok(ApiResponse.Success(user, ApiMessages.CurrentUserFetched));
    }

    [HttpPost("mcp-token")]
    public async Task<IActionResult> CreateMcpToken(CancellationToken cancellationToken)
    {
        var result = await authService.CreateMcpTokenAsync(cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.McpTokenCreated));
    }

    private void WriteRefreshCookie(AuthResult result) =>
        Response.Cookies.Append(RefreshCookieName, result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = RefreshCookiePath,
            Expires = result.RefreshTokenExpiresAt,
        });

    private void ClearRefreshCookie() =>
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = RefreshCookiePath,
        });
}
