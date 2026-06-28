using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Core.Types;
using Backend.Core.Common;
using Microsoft.AspNetCore.Mvc;
using Backend.Services.Users;
using Sqids;

namespace Backend.Endpoints;

public static class UserEndpoints
{
    public static void AddUserEndpoints(this IEndpointRouteBuilder route)
    {
        var user = route.MapGroup("/api/user").WithTags("Users");

        user.MapGet("{id}", HandleGetById).RequireAuthorization();
        user.MapGet("{id}/stat", HandleGetUserStat).RequireAuthorization();
        user.MapGet("query", HandleQuery).RequireAuthorization();

        user.MapPut("", HandleCreate).RequireAuthorization();
        user.MapPost("{id}", HandleUpdate).RequireAuthorization();
        user.MapDelete("{id}", HandleDelete).RequireAuthorization();

        user.MapGet("{id}/check", HandleCheckActive).RequireAuthorization();
        user.MapPost("{id}/activate", HandleActivate).RequireAuthorization();
        user.MapPost("{id}/deactivate", HandleDeactivate).RequireAuthorization();
    }

    private static async
        Task<Results<Ok<UserDetailResponse>, BadRequest, NotFound>>
        HandleGetById(
            string id,
            SqidsEncoder<long> sqidsEncoder,
            UserService userService)
    {
        var decoded = sqidsEncoder.Decode(id);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await userService.GetUserByIdAsync(decoded[0]);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<UserStatResponse>, BadRequest, NotFound>>
        HandleGetUserStat(
            string id,
            SqidsEncoder<long> sqidsEncoder,
            UserService userService)
    {
        var decoded = sqidsEncoder.Decode(id);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await userService.GetUserStatAsync(decoded[0]);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Ok<QueryResponse<UserResponse>>>
        HandleQuery(
            [AsParameters] UserRequest query,
            UserService userService)
    {
        var results = await userService.QueryUsersAsync(query);
        return TypedResults.Ok(results);
    }

    private static async
        Task<Ok<UserSetResponse>>
        HandleCreate(
            [FromBody] UserSetRequest request,
            UserService userService)
    {
        var result = await userService.CreateUserAsync(request);
        return TypedResults.Ok(result);
    }

    private static async
        Task<Results<Ok<UserSetResponse>, BadRequest, NotFound>>
        HandleUpdate(
            string id,
            [FromBody] UserSetRequest request,
            SqidsEncoder<long> sqidsEncoder,
            UserService userService)
    {
        var decoded = sqidsEncoder.Decode(id);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await userService.UpdateUserAsync(decoded[0], request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, BadRequest, NotFound>>
        HandleDelete(
            string id,
            SqidsEncoder<long> sqidsEncoder,
            UserService userService)
    {
        var decoded = sqidsEncoder.Decode(id);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await userService.DeleteUserAsync(decoded[0]);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<bool>, BadRequest>>
        HandleCheckActive(
            string id,
            SqidsEncoder<long> sqidsEncoder,
            UserService userService)
    {
        var decoded = sqidsEncoder.Decode(id);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await userService.IsUserActiveAsync(decoded[0]);
        return TypedResults.Ok(result);
    }

    private static async
        Task<Results<Ok, BadRequest, NotFound>>
        HandleActivate(
            string id,
            SqidsEncoder<long> sqidsEncoder,
            UserService userService)
    {
        var decoded = sqidsEncoder.Decode(id);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await userService.ActivateUserAsync(decoded[0]);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, BadRequest, NotFound>>
        HandleDeactivate(
            string id,
            SqidsEncoder<long> sqidsEncoder,
            UserService userService)
    {
        var decoded = sqidsEncoder.Decode(id);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await userService.DeactivateUserAsync(decoded[0]);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }
}
