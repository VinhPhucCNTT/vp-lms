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
    public Task<LessonResponse?> GetLessonByIdAsync(long resourceId);

    public Task<LessonResponse> CreateAsync(long moduleId, LessonRequest request, CancellationToken ct = default);

    public Task<LessonResponse?> UpdateLessonAsync(long resourceId, LessonRequest request);

    public Task<Result<bool>> SetPublishStatusAsync(long resourceId, bool isPublished, CancellationToken ct = default);
}

public class LessonService(
    IDbContextFactory<AppDbContext> dbFactory,
    IMapper mapper) : ILessonService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly IMapper _mapper = mapper;

    public async Task<LessonResponse?> GetLessonByIdAsync(long resourceId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var lesson = await db.Lessons
            .AsNoTracking()
            .Include(l => l.Resource)
            .Where(l => l.ResourceId == resourceId)
            .FirstOrDefaultAsync();

        return lesson is null
            ? null
            : new LessonResponse(
                _mapper.Map<ResourceDetailResponse>(lesson.Resource),
                new LessonInfo(lesson.ContentMarkdown));
    }

    public async Task<LessonResponse> CreateAsync(long moduleId, LessonRequest request, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var resource = await ResourceService.CreateResourceAsync(db, moduleId, request.ResourceInfo, ResourceType.Lesson, ct);
        var lesson = new Lesson
        {
            ResourceId = resource.Id,
            ContentMarkdown = request.Info.ContentMarkdown
        };

        db.Lessons.Add(lesson);
        await db.SaveChangesAsync(ct);

        return new LessonResponse(
            _mapper.Map<ResourceDetailResponse>(resource),
            new LessonInfo(lesson.ContentMarkdown)
        );
    }

    public async Task<LessonResponse?> UpdateLessonAsync(long resourceId, LessonRequest request)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var lesson = await db.Lessons
            .FirstOrDefaultAsync(r => r.ResourceId == resourceId);
        if (lesson is null)
            return null;

        var resource = await ResourceService.UpdateResourceAsync(db, lesson.ResourceId, request.ResourceInfo);

        lesson.ContentMarkdown = request.Info.ContentMarkdown;
        db.Lessons.Update(lesson);
        await db.SaveChangesAsync();

        return new LessonResponse(
            _mapper.Map<ResourceDetailResponse>(resource),
            new LessonInfo(lesson.ContentMarkdown));
    }

    public async Task<Result<bool>> SetPublishStatusAsync(long resourceId, bool isPublished, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var resource = await db.CourseResources.FirstOrDefaultAsync(r => r.Type == ResourceType.Lesson && r.Id == resourceId, ct);
        if (resource is null)
            return Result<bool>.Failure(ResultError.NotFound, "Lesson not found.");

        resource.IsPublished = isPublished;
        await db.SaveChangesAsync(ct);

        return Result<bool>.Success(resource.IsPublished);
    }
}
