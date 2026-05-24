namespace Backend.Features.Auth.Dtos;

public record LoginRequest(
    string Email,
    string Password
);

public record LoginResponse(
    string Email,
    string Token
);
