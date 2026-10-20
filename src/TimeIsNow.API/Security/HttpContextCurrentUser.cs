using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TimeIsNow.Application.Interfaces.Security;

namespace TimeIsNow.API.Security;

public sealed class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public int UserId => int.TryParse(
        Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : 0;

    public string? Email => Principal?.FindFirstValue(JwtRegisteredClaimNames.Email);
}
