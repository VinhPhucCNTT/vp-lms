using System.Security.Claims;
using Backend.Models.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Endpoints;

public static class AuthAdminEndpoints
{
    public record AdminUserResponse(
        Guid Id,
        string? Email,
        string? UserName,
        string FullName,
        DateTime CreatedAt,
        string Role
    );

    public static void AddAuthAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("api/auth");

        auth.MapDelete("{email}", HandleDeleteUser);
        auth.MapGet("", HandleGetUsers);
        auth.MapGet("check", HandleCheck).RequireAuthorization();
        app.MapGet("/debug-auth", (ClaimsPrincipal user) =>
        {
            return user.Claims.Select(c => new
            {
                c.Type,
                c.Value
            });
        }).RequireAuthorization();
    }

    private static async
        Task<Results<Ok, BadRequest>>
        HandleDeleteUser(
            string email,
            UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return TypedResults.BadRequest();

        await userManager.DeleteAsync(user);
        return TypedResults.Ok();
    }

    private static async Task<List<AdminUserResponse>>
        HandleGetUsers(UserManager<ApplicationUser> userManager)
    {
        var users = await userManager.Users.ToListAsync();

        var result = new List<AdminUserResponse>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);

            result.Add(new AdminUserResponse(
                user.Id,
                user.Email,
                user.UserName,
                user.FullName,
                user.CreatedAt,
                roles.FirstOrDefault() ?? "No Role"
            ));
        }

        return result;
    }

    private static Ok HandleCheck() => TypedResults.Ok();
}
