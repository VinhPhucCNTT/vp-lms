using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Submissions;
using Backend.Api.Services.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Sqids;

namespace Backend.Api.Endpoints;

public static class DevelopmentOnlyEndpoints
{
    public static void AddDevEndpoints(this IEndpointRouteBuilder route)
    {
        var dev = route.MapGroup("/api/dev");

        dev.MapGet("users", HandleGetUsers);
        dev.MapGet("auth-test", async () => TypedResults.Ok()).RequireAuthorization();
        dev.MapPost("judge-test/{resourceId}", HandleTestJudge);
    }

    private static async
        Task<Ok<PaginatedResponse<UserResponse>>>
        HandleGetUsers(
            [AsParameters] UserRequest query,
            IUserService userService)
    {
        var results = await userService.QueryUsersAsync(query);
        return TypedResults.Ok(results);
    }

    private static async
        Task<IResult>
        HandleTestJudge(
            string resourceId,
            [AsParameters] JudgeTestRequest request,
            SqidsEncoder<long> sqidsEncoder,
            JudgeSubmissionService judgeSubmissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await judgeSubmissionService.Send(decoded[0], request, ct);
        if (!result.IsSuccess)
            switch (result.Error?.Code)
            {
                case "not_found":
                    return TypedResults.NotFound();
                case "forbidden":
                    return TypedResults.Forbid();
                case "internal":
                    return TypedResults.InternalServerError();
                default:
                    return TypedResults.InternalServerError();
            }

        return TypedResults.Ok(result.Value);
    }
}
