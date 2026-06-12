using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Core.Types;
using Backend.Core.Entities.Courses;
using Sqids;
using Backend.Core.Entities.Resources;

namespace Backend.Services.Content;

public class LessonService(
    IDbContextFactory<AppDbContext> dbFactory,
    // CurrentUserService currentUserService,
    SqidsEncoder<long> sqidsEncoder)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    // private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly SqidsEncoder<long> _sqidsEncoder = sqidsEncoder;

    public async Task<LessonResponse?> GetLessonByIdAsync(long resourceId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Lessons
            .AsNoTracking()
            .Where(l => l.ResourceId == resourceId)
            .Select(l => new LessonResponse(
                _sqidsEncoder.Encode(l.Id),
                l.ContentMarkdown)
            ).FirstOrDefaultAsync();
    }

    public async Task<LessonResponse> CreateLessonAsync(ModuleResource resource, LessonCreateRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var lesson = new Lesson
        {
            ResourceId = resource.Id,
            ContentMarkdown = request.ContentMarkdown
        };
        db.Lessons.Add(lesson);
        await db.SaveChangesAsync();
        return new LessonResponse(
            _sqidsEncoder.Encode(lesson.Id),
            lesson.ContentMarkdown
        );
    }

    public async Task<LessonResponse?> UpdateLessonAsync(long lessonId, LessonUpdateRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var lesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson is null)
            return null;

        lesson.ContentMarkdown = request.ContentMarkdown;
        db.Lessons.Update(lesson);
        await db.SaveChangesAsync();

        return new LessonResponse(
            _sqidsEncoder.Encode(lesson.Id),
            lesson.ContentMarkdown
        );
    }
}
