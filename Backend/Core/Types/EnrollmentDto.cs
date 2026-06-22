using Backend.Core.Entities.Courses;

namespace Backend.Core.Types;

public record EnrollmentResponse(
    string Id,
    string CourseId,
    string UserId,
    EnrollmentRole Role
) : IEntityResponse;

public record EnrollmentDetailResponse(
    string Id,
    string CourseId,
    UserResponse User,
    EnrollmentRole Role,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IEntityResponse;
