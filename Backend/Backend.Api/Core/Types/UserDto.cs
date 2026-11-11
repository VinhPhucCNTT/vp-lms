using Backend.Persistence.Entities.Users;

namespace Backend.Api.Core.Types;

public record UserResponse(
    string Id,
    UserRoles Role,
    string Email,
    string FirstName,
    string LastName,
    string? AvatarFileId) : IEntityResponse;

public record UserDetailResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string? AvatarFileId,
    UserRoles Role,
    DateTime CreatedAt
) : IEntityResponse;

public record UserQueryRequest(
    int PageNumber,
    int PageSize,
    string? Email,
    string? FirstName,
    string? LastName,
    UserRoles? Role
);

public record UserStatResponse(
    int CourseCreated,
    int CourseEnrolled
// int CourseCompleted // TODO: Get completed courses
);

public record UserCreateRequest(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    IFormFile AvatarFile,
    UserRoles Role
);

public record UserPatchRequest(
    string? Email,
    string? FirstName,
    string? LastName,
    string? Password,
    IFormFile? AvatarFile
);
