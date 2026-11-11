using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Sqids;

namespace Backend.Api.Endpoints;

public static class UserEndpoints
{
    public static void AddUserEndpoints(this IEndpointRouteBuilder route)
    {
        var user = route.MapGroup("/api/user").WithTags("Users");

        user.MapGet("{id}", HandleGetById).RequireAuthorization();
        user.MapGet("{id}/stat", HandleGetUserStat).RequireAuthorization();
        user.MapGet("query", HandleQuery).RequireAuthorization();

        user.MapPost("", HandleCreate).RequireAuthorization();
        user.MapPatch("{id}", HandleUpdate).RequireAuthorization();
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
            [FromServices] IUserService userService)
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
            [FromServices] IUserService userService)
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
        Task<Ok<PaginatedResponse<UserResponse>>>
        HandleQuery(
            [AsParameters] UserQueryRequest query,
            [FromServices] IUserService userService)
    {
        var results = await userService.QueryUsersAsync(query);
        return TypedResults.Ok(results);
    }

    private static async
        Task<Ok<UserSetResponse>>
        HandleCreate(
            [FromForm] UserCreateRequest request,
            [FromServices] IUserService userService)
    {
        var result = await userService.CreateUserAsync(request);
        return TypedResults.Ok(result);
    }

    private static async
        Task<Results<Ok<UserPatchResponse>, BadRequest, NotFound, InternalServerError>>
        HandleUpdate(
            string id,
            [FromForm] UserPatchRequest request,
            SqidsEncoder<long> sqidsEncoder,
            [FromServices] IUserService userService)
    {
        var decoded = sqidsEncoder.Decode(id);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await userService.UpdateUserAsync(decoded[0], request);
        if (!result.IsSuccess)
        {
            return result.Error!.Code switch
            {
                "user.upload" => (Results<Ok<UserPatchResponse>, BadRequest, NotFound, InternalServerError>)TypedResults.InternalServerError(),
                // "user.not_found"
                _ => (Results<Ok<UserPatchResponse>, BadRequest, NotFound, InternalServerError>)TypedResults.NotFound(),
            };
        }

        return TypedResults.Ok(result.Value);
    }

    private static async
        Task<Results<Ok, BadRequest, NotFound>>
        HandleDelete(
            string id,
            SqidsEncoder<long> sqidsEncoder,
            [FromServices] IUserService userService)
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
            [FromServices] IUserService userService)
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
            [FromServices] IUserService userService)
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
            [FromServices] IUserService userService)
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
