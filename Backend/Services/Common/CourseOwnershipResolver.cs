using System.Security.Claims;
using Backend.Core.Types;
using Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Common;

public class CourseOwnershipResolver(
    IDbContextFactory<AppDbContext> dbFactory,
    IAuthorizationService authService,
    IHttpContextAccessor httpContextAccessor)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly IAuthorizationService _authService = authService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
}
