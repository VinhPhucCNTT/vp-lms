using Backend.Api.Core.Entities.Users;

namespace Backend.Api.Core.Types;

public record UserResponse(
    string Id,
    string Username,
    UserRoles Role,
    string? AvatarUrl) : IEntityResponse;

public record UserDetailResponse(
    string Id,
    string Username,
    string Email,
    string Fullname,
    string? AvatarUrl,
    UserRoles Role,
    DateTime CreatedAt
) : IEntityResponse;

public record UserRequest(
    int PageNumber,
    int PageSize,
    string? Username,
    string? Email,
    string? Fullname,
    UserRoles? Role
);

public record UserStatResponse(
    int CourseCreated,
    int CourseEnrolled
// int CourseCompleted // TODO: Get completed courses
);

public record UserCreateRequest(
    string Username,
    string Email,
    string Fullname,
    string Password,
    string? AvatarUrl,
    UserRoles Role
);

public record UserUpdateRequest(
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
    string? AvatarUrl,
    UserRoles Role
) : IEntityResponse;
