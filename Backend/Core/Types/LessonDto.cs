namespace Backend.Core.Types;

public record LessonCreateRequest(
    string ContentMarkdown
);

public record LessonUpdateRequest(string ContentMarkdown);

public record LessonResponse(
    string LessonSqid,
    string ContentMarkdown
);
