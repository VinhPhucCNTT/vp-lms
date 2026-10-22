using Backend.Persistence.Entities.Content;
using Backend.Persistence.Data;
using Backend.Api.Services.Common;
using Backend.Api.Services.Courses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Authorization;

public interface IAssessmentAuthorization
{
}

public class AssessmentAuthorization(
    CurrentUserService currentUserService,
    IDbContextFactory<AppDbContext> dbFactory,
    ICourseService courseService,
    ICourseAuthorization courseAuthorization) : IAssessmentAuthorization
{
    private readonly CurrentUserService currentUserService = currentUserService;
    private readonly IDbContextFactory<AppDbContext> dbFactory = dbFactory;
    private readonly ICourseService courseService = courseService;
    private readonly ICourseAuthorization courseAuthorization = courseAuthorization;

    // public async Task<bool> CanModifyQuestionAsync(Assessment assessment, CancellationToken ct = default)
    // {
    //     var userId = currentUserService.UserId;
    //     if (
    // }
}
