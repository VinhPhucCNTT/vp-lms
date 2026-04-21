namespace Backend.Features.Lessons.Dtos;

public record ViewLessonDto(
    Guid Id,
    string ContentHtml,
    string? VideoUrl,
    string? AttachmentUrl
);
