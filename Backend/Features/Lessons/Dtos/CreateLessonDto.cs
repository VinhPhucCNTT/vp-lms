namespace Backend.Features.Lessons.Dtos;

public record CreateLessonDto(
    string ContentHtml,
    string? VideoUrl,
    string? AttachmentUrl
);
