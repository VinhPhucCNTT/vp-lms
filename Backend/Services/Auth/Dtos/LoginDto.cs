namespace Backend.Services.Auth.Dtos;

public record LoginRequest(
    string Email,
    string Password
);

public record LoginResponse(
    string Email,
    string Token
);
