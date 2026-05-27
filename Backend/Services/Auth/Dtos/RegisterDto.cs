namespace Backend.Services.Auth.Dtos;

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
