using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Services.Auth.Dtos;
using Backend.Services.Auth.Services;

namespace Backend.Endpoints;

public static class AuthEndpoints
{
    public static void AddAuthEndpoints(this IEndpointRouteBuilder route)
    {
        var auth = route.MapGroup("/api/auth");

        auth.MapGet("", async () => TypedResults.Ok).RequireAuthorization();
        auth.MapPost("login", HandleLogin);
        auth.MapPost("register", HandleRegister);
    }

    private static async
        Task<Results<Ok<LoginResponse>, BadRequest>>
        HandleLogin(LoginRequest dto, IAuthService authService)
    {
        var response = await authService.LoginAsync(dto);
        if (response is null)
            return TypedResults.BadRequest();

        return TypedResults.Ok(response);
    }

    private static async
        Task<Results<Ok<RegisterResponse>, BadRequest>>
        HandleRegister(RegisterRequest dto, IAuthService authService)
    {
        var response = await authService.RegisterAsync(dto);
        if (response is null)
            return TypedResults.BadRequest();

        return TypedResults.Ok(response);
    }
}
