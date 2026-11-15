namespace Backend.Api.Core.Types;

public record CourseAuthorizationActivity(
    long CourseId,
    long CreatorId
);
