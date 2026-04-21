namespace Backend.Features.Lessons.Dtos;

public record UpdateLessonDto(
    string ContentHtml,
    string? VideoUrl,
    string? AttachmentUrl
);
