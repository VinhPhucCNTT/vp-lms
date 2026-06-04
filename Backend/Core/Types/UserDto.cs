using Backend.Core.Entities.Users;

namespace Backend.Core.Types;

public record UserResponse(
    long UserId,
    string Username,
    string? AvatarUrl)
{
    public static UserResponse Set(User u)
        => new(u.Id, u.Username, u.AvatarUrl);
}

public record UserDetailResponse(
    long UserId,
    string Username,
    string Email,
    string Fullname,
    string? AvatarUrl,
    DateTime CreatedAt
);

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
    long UserId,
    string Username,
    string Email,
    string Fullname,
    string? AvatarUrl
);
