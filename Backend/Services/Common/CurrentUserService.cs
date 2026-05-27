using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Backend.Services.Common;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?
                .User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
        }
    }

    public string Email =>
        _httpContextAccessor.HttpContext?
            .User?.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? string.Empty;

    public string FullName =>
        _httpContextAccessor.HttpContext?
            .User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    // TODO: Handle admin role
    // public bool IsInRole(string role) =>
    //     _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
}
