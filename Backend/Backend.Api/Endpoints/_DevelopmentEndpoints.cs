using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Users;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Api.Endpoints;

public static class DevelopmentOnlyEndpoints
{
    public static void AddDevEndpoints(this IEndpointRouteBuilder route)
    {
        var dev = route.MapGroup("/api/dev");

        dev.MapGet("users", HandleGetUsers);
        dev.MapGet("auth-test", async () => TypedResults.Ok()).RequireAuthorization();
    }

    private static async
        Task<Ok<PaginatedResponse<UserResponse>>>
        HandleGetUsers(
            [AsParameters] UserRequest query,
            UserService userService)
    {
        var results = await userService.QueryUsersAsync(query);
        return TypedResults.Ok(results);
    }
}
