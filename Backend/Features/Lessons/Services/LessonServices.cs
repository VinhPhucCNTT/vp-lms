using Backend.Data.Repositories;
using Backend.Data.UnitOfWork;
using Backend.Features.Lessons.Dtos;
using Backend.Models.Lessons;

namespace Backend.Features.Lessons.Services;

public class LessonService(
        ILessonRepository lessonRepo,
        IUnitOfWork uow) : ILessonService
{
    private readonly ILessonRepository _lessonRepo = lessonRepo;
    private readonly IUnitOfWork _uow = uow;

    public async Task<Guid> CreateAsync(Guid activityId, CreateLessonDto dto)
    {
        var lesson = new Lesson
        {
            ActivityId = activityId,
            ContentHtml = dto.ContentHtml,
            VideoUrl = dto.VideoUrl,
            AttachmentUrl = dto.AttachmentUrl
        };

        _lessonRepo.Add(lesson);

        await _uow.SaveChangesAsync();
        return lesson.Id;
    }

    public async Task<ViewLessonDto?> GetByIdAsync(Guid id)
    {
        var lesson = await _lessonRepo.GetByIdAsync(id);
        if (lesson is null)
            return null;

        return new ViewLessonDto(
            lesson.Id,
            lesson.ContentHtml,
            lesson.VideoUrl,
            lesson.AttachmentUrl);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateLessonDto dto)
    {
        var lesson = await _lessonRepo.GetByIdAsync(id);
        if (lesson is null)
            return false;

        lesson.ContentHtml = dto.ContentHtml;
        lesson.VideoUrl = dto.VideoUrl;
        lesson.AttachmentUrl = dto.AttachmentUrl;

        _lessonRepo.Update(lesson);

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var lesson = await _lessonRepo.GetByIdAsync(id);
        if (lesson is null)
            return false;

        _lessonRepo.Remove(lesson);

        await _uow.SaveChangesAsync();
        return true;
    }
}
