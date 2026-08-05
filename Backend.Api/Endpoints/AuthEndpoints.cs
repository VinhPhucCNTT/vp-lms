using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Api.Core.Types;
using Backend.Api.Services.Auth;

namespace Backend.Api.Endpoints;

public static class AuthEndpoints
{
    public static void AddAuthEndpoints(this IEndpointRouteBuilder route)
    {
        var auth = route.MapGroup("/api/auth").WithTags("Authentication");

        auth.MapGet("/", async () => TypedResults.Ok).WithDescription("Authentication check.").RequireAuthorization();
        auth.MapPost("login", HandleLogin).WithDescription("Login.");
        auth.MapPost("register", HandleRegister).WithDescription("Create an account.");
    }

    private static async
        Task<Results<Ok<LoginResponse>, BadRequest>>
        HandleLogin(LoginRequest dto, AuthenticationService authService)
    {
        var response = await authService.LoginAsync(dto);
        if (response is null)
            return TypedResults.BadRequest();

        return TypedResults.Ok(response);
    }

    private static async
        Task<Results<Ok<RegisterResponse>, BadRequest>>
        HandleRegister(RegisterRequest dto, AuthenticationService authService)
    {
        var response = await authService.RegisterAsync(dto);
        if (response is null)
            return TypedResults.BadRequest();

        return TypedResults.Ok(response);
    }
}
