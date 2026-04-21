using Backend.Models.Users;
using Backend.Features;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Backend.Data;

namespace Backend.Endpoints;

public static class AuthEndpoints
{
    public record UserLoginRequest(string Email, string Password);
    public record UserLoginResponse(string Email, string Token);

    public record UserRegisterRequest(string FullName, string Email, string Username, string Password, string Role);
    public record UserRegisterResponse(string Email);

    public record UserResponse(
        Guid Id,
        string? Email,
        string? UserName,
        string FullName,
        DateTime CreatedAt,
        string Role
    );

    public static void AddAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth");

        auth.MapPost("login", HandleLogin);
        auth.MapPost("register", HandleRegister);
        auth.MapGet("students", HandleGetStudents);
        auth.MapGet("instructors", HandleGetInstructors);
    }

    private static async
        Task<Results<Ok<UserLoginResponse>, UnauthorizedHttpResult>>
        HandleLogin(
            UserLoginRequest request,
            UserManager<ApplicationUser> userManager,
            JwtService jwtService)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return TypedResults.Unauthorized();

        if (!await userManager.CheckPasswordAsync(user, request.Password))
            return TypedResults.Unauthorized();

        var token = await jwtService.GenerateToken(user);
        return TypedResults.Ok<UserLoginResponse>(new(request.Email, token));
    }

    private static async
        Task<Results<Ok<UserRegisterResponse>, UnauthorizedHttpResult, BadRequest<List<IdentityError>>>>
        HandleRegister(
            UserRegisterRequest request,
            AppDbContext dbContext,
            UserManager<ApplicationUser> userManager)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        if (request.Role != "Student" && request.Role != "Instructor")
            return TypedResults.Unauthorized();

        var exist = await userManager.FindByEmailAsync(request.Email);
        if (exist is not null)
            return TypedResults.Unauthorized();

        var user = new ApplicationUser
        {
            FullName = request.FullName,
            UserName = request.Username,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return TypedResults.BadRequest(result.Errors.ToList());
        }

        await userManager.AddToRoleAsync(user, request.Role);

        await transaction.CommitAsync();

        return TypedResults.Ok<UserRegisterResponse>(new(request.Email));
    }

    private static async Task<List<UserResponse>>
        HandleGetStudents(UserManager<ApplicationUser> userManager)
    {
        return await HandleGetUsersInRole("Student", userManager);
    }

    private static async Task<List<UserResponse>>
        HandleGetInstructors(UserManager<ApplicationUser> userManager)
    {
        return await HandleGetUsersInRole("Instructor", userManager);
    }

    private static async Task<List<UserResponse>>
        HandleGetUsersInRole(string role, UserManager<ApplicationUser> userManager)
    {
        var users = await userManager.GetUsersInRoleAsync(role);

        var result = new List<UserResponse>();

        foreach (var user in users)
        {
            result.Add(new UserResponse(
                user.Id,
                user.Email,
                user.UserName,
                user.FullName,
                user.CreatedAt,
                role
            ));
        }

        return result;
    }

}
