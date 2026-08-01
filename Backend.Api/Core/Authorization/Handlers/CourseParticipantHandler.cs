using System.IdentityModel.Tokens.Jwt;
using Backend.Api.Core.Authorization.Requirements;
using Backend.Api.Core.Entities.Users;
using Backend.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Authorization.Handlers;

public sealed class CourseParticipantHandler(
    IHttpContextAccessor httpContextAccessor,
    IDbContextFactory<AppDbContext> dbFactory
): AuthorizationHandler<CourseParticipantRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CourseParticipantRequirement requirement)
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
        if (!long.TryParse(
            httpContext.Request.RouteValues["courseId"]?.ToString(),
            out var courseId))
            return;

        using var db = await _dbFactory.CreateDbContextAsync();
        if (!await db.Enrollments
                .AsNoTracking()
                .AnyAsync(e => e.CourseId == courseId && e.UserId == userId))
            return;
        context.Succeed(requirement);
    }
}
