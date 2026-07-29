namespace Backend.Api.Core.Types;

public record LessonInfo(
    string ContentMarkdown
);

public record LessonRequest(
    ResourceRequestInfo ResourceInfo,
    LessonInfo Info
);

public record LessonResponse(
    ResourceDetailResponse ResourceInfo,
    LessonInfo Info
);
