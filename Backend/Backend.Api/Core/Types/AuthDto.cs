using Backend.Persistence.Entities.Users;

namespace Backend.Api.Core.Types;

public record LoginRequest(
    string Email,
    string Password
);

public record LoginResponse(
    string Email,
    string Token,
    UserRoles Role
);

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string Fullname,
    string? AvatarUrl,
    UserRoles Role
);

public record RegisterResponse(
    string Username,
    string Email,
    UserRoles Role
);
