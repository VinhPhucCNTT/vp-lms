using Backend.Api.Core.Entities.Content;
using Backend.Api.Data;
using Backend.Api.Services.Common;
using Backend.Api.Services.Courses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Authorization;

public class AssessmentAuthorization(
    CurrentUserService currentUserService,
    IDbContextFactory<AppDbContext> dbFactory,
    CourseService courseService,
    CourseAuthorization courseAuthorization)
{
    private readonly CurrentUserService currentUserService = currentUserService;
    private readonly IDbContextFactory<AppDbContext> dbFactory = dbFactory;
    private readonly CourseService courseService = courseService;
    private readonly CourseAuthorization courseAuthorization = courseAuthorization;

    // public async Task<bool> CanModifyQuestionAsync(Assessment assessment, CancellationToken ct = default)
    // {
    //     var userId = currentUserService.UserId;
    //     if (
    // }
}
