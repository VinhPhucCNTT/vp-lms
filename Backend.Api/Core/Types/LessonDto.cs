namespace Backend.Api.Core.Types;

public record LessonInfo(
    string ContentMarkdown
);

public record LessonRequest(
    ResourceRequestInfo ResourceInfo,
    LessonInfo Info
);

public record LessonSetResponse(
    string Id,
    ResourceDetailResponse ResourceInfo,
    LessonInfo Info
) : IEntityResponse;
