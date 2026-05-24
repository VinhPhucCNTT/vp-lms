using Backend.Features.Auth.Dtos;

namespace Backend.Features.Auth.Services;

public interface IAuthService
{
    // Login and returns token
    Task<LoginResponse?> LoginAsync(LoginRequest dto);

    // Register and creates a new user
    Task<RegisterResponse?> RegisterAsync(RegisterRequest dto);
}
