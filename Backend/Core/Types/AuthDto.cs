namespace Backend.Core.Types;

public record LoginRequest(
    string Email,
    string Password
);

public record LoginResponse(
    string Email,
    string Token
);

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string Fullname,
    string? AvatarUrl
);

public record RegisterResponse(
    string Username,
    string Email
);
