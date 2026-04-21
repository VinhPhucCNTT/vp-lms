using Backend.Features.Lessons.Dtos;

namespace Backend.Features.Lessons.Services;

public interface ILessonService
{
    Task<Guid> CreateAsync(Guid activityId, CreateLessonDto dto);

    Task<ViewLessonDto?> GetByIdAsync(Guid id);

    Task<bool> UpdateAsync(Guid id, UpdateLessonDto dto);

    Task<bool> DeleteAsync(Guid id);
}
