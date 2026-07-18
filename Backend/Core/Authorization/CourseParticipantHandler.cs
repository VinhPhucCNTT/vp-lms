using System.Security.Claims;
using Backend.Core.Entities.Users;
using Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Authorization;

public sealed class CourseParticipantHandler(
    IDbContextFactory<AppDbContext> dbFactory,
    IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<CourseParticipantRequirement>
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CourseParticipantRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return;

        var role = httpContext.User.FindFirst(ClaimTypes.Role);
        if (role != null && role.Equals(UserRoles.Admin.ToString()))
        {
            context.Succeed(requirement);
            return;
        }

        if (!long.TryParse(
            httpContext.Request.RouteValues["courseId"]?.ToString(),
            out var courseId))
            return;

        if (!long.TryParse(
            httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            out var userId))
            return;

        var db = await _dbFactory.CreateDbContextAsync();
        bool isParticipant = await db.Enrollments
            .AsNoTracking()
            .AnyAsync(e =>
                e.CourseId == courseId &&
                e.UserId == userId);
        if (isParticipant)
            context.Succeed(requirement);
    }
}
