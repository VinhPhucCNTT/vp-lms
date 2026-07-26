using Backend.Api.Core.Entities.Users;

namespace Backend.Api.Core.Types;

public record UserResponse(
    string Id,
    string Username,
    string? AvatarUrl) : IEntityResponse;

public record UserDetailResponse(
    string Id,
    string Username,
    string Email,
    string Fullname,
    string? AvatarUrl,
    DateTime CreatedAt
) : IEntityResponse;

public record UserRequest(
    int PageNumber,
    int PageSize,
    string? Username,
    string? Email,
    string? Fullname
);

public record UserStatResponse(
    int CourseCreated,
    int CourseEnrolled
// int CourseCompleted // TODO: Get completed courses
);

public record UserSetRequest(
    string Username,
    string Email,
    string Fullname,
    string Password,
    string? AvatarUrl
);

public record UserSetResponse(
    string Id,
    string Username,
    string Email,
    string Fullname,
    string? AvatarUrl
) : IEntityResponse;
