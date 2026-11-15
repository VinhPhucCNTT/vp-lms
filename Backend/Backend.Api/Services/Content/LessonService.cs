using AutoMapper;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Courses;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Content;
using Backend.Persistence.Entities.Courses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Content;

public interface ILessonService
{
    public Task<LessonResponse?> GetLessonByIdAsync(long activityId);

    public Task<LessonResponse> CreateAsync(long moduleId, LessonRequest request, CancellationToken ct = default);

    public Task<LessonResponse?> UpdateLessonAsync(long activityId, LessonRequest request);

    public Task<Result<bool>> SetPublishStatusAsync(long activityId, bool isPublished, CancellationToken ct = default);
}

public class LessonService(
    IDbContextFactory<AppDbContext> dbFactory,
    IMapper mapper) : ILessonService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly IMapper _mapper = mapper;

    public async Task<LessonResponse?> GetLessonByIdAsync(long activityId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var lesson = await db.Lessons
            .AsNoTracking()
            .Include(l => l.Activity)
            .Where(l => l.ActivityId == activityId)
            .FirstOrDefaultAsync();

        return lesson is null
            ? null
            : new LessonResponse(
                _mapper.Map<ActivityDetailResponse>(lesson.Activity),
                new LessonInfo(lesson.ContentMarkdown));
    }

    public async Task<LessonResponse> CreateAsync(long moduleId, LessonRequest request, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var activity = await ActivityService.CreateActivityAsync(db, moduleId, request.ActivityInfo, ActivityType.Lesson, ct);
        var lesson = new Lesson
        {
            ActivityId = activity.Id,
            ContentMarkdown = request.Info.ContentMarkdown
        };

        db.Lessons.Add(lesson);
        await db.SaveChangesAsync(ct);

        return new LessonResponse(
            _mapper.Map<ActivityDetailResponse>(activity),
            new LessonInfo(lesson.ContentMarkdown)
        );
    }

    public async Task<LessonResponse?> UpdateLessonAsync(long activityId, LessonRequest request)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var lesson = await db.Lessons
            .FirstOrDefaultAsync(r => r.ActivityId == activityId);
        if (lesson is null)
            return null;

        var activity = await ActivityService.UpdateActivityAsync(db, lesson.ActivityId, request.ActivityInfo);

        lesson.ContentMarkdown = request.Info.ContentMarkdown;
        db.Lessons.Update(lesson);
        await db.SaveChangesAsync();

        return new LessonResponse(
            _mapper.Map<ActivityDetailResponse>(activity),
            new LessonInfo(lesson.ContentMarkdown));
    }

    public async Task<Result<bool>> SetPublishStatusAsync(long activityId, bool isPublished, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var activity = await db.CourseActivities.FirstOrDefaultAsync(r => r.Type == ActivityType.Lesson && r.Id == activityId, ct);
        if (activity is null)
            return Result<bool>.Failure(ResultError.NotFound, "Lesson not found.");

        activity.IsPublished = isPublished;
        await db.SaveChangesAsync(ct);

        return Result<bool>.Success(activity.IsPublished);
    }
}
