using Backend.Persistence.Entities.Users;

namespace Backend.Api.Core.Types;

public record LoginRequest(
    string Email,
    string Password
);

public record LoginResponse(
    string Email,
    string Token,
    string FirstName,
    string LastName,
    string? AvatarFileId,
    bool IsActive,
    UserRoles Role
);

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    UserRoles Role
);

public record RegisterResponse(
    string Email,
    string FirstName,
    string LastName,
    UserRoles Role
);
