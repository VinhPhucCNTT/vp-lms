namespace Backend.Api.Core.Types;

public enum FileCategory
{
    Avatar,
    CourseThumbnail,
    CourseBackground,
    LessonAttachment,
    AssignmentSubmission,
    QuestionImage
}

public record FileResponse(
    string Id,
    string UserId,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    string Sha256Hash
) : IEntityResponse;
