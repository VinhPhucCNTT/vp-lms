using System.IdentityModel.Tokens.Jwt;
using Backend.Api.Core.Authorization.Requirements;
using Backend.Persistence.Entities.Users;
using Backend.Api.Core.Types;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Api.Core.Authorization.Handlers;

public sealed class CourseOwnerHandler(
    IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<CourseOwnerRequirement, CourseAuthorizationResource>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CourseOwnerRequirement requirement,
        CourseAuthorizationResource resource)
    {
        if (context.User.IsInRole(UserRoles.Admin.ToString()))
        {
            context.Succeed(requirement);
            return;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return;

        if (!long.TryParse(
            httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
            out var userId))
            return;

        if (resource.CreatorId == userId)
            context.Succeed(requirement);
    }
}
