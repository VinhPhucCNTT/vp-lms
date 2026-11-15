namespace Backend.Api.Core.Types;

public record LessonInfo(
    string ContentMarkdown
);

public record LessonRequest(
    ActivityRequestInfo ActivityInfo,
    LessonInfo Info
);

public record LessonResponse(
    ActivityDetailResponse ActivityInfo,
    LessonInfo Info
);
