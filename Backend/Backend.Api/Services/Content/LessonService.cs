using Backend.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Content;
using AutoMapper;
using Backend.Api.Services.Courses;
using Backend.Api.Core.Common;

namespace Backend.Api.Services.Content;

public class LessonService(
    IDbContextFactory<AppDbContext> dbFactory,
    IMapper mapper)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly IMapper _mapper = mapper;

    public async Task<LessonResponse?> GetLessonByIdAsync(long resourceId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
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
        using var db = await _dbFactory.CreateDbContextAsync(ct);
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
        using var db = await _dbFactory.CreateDbContextAsync();
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
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var resource = await db.CourseResources.FirstOrDefaultAsync(r => r.Type == ResourceType.Lesson && r.Id == resourceId, ct);
        if (resource is null)
            return Result<bool>.Failure(ErrorType.NotFound, "Lesson not found.");

        resource.IsPublished = isPublished;
        await db.SaveChangesAsync(ct);

        return Result<bool>.Success(resource.IsPublished);
    }
}
