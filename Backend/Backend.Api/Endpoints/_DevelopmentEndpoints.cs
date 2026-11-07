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
        var dev = route.MapGroup("/api/dev").WithTags("Development");

        dev.MapGet("users", HandleGetUsers);
        dev.MapPost("judge-test/{resourceId}", HandleTestJudge);
        dev.MapGet("current-date-utc", HandleGetUtc);
    }

    private static async
        Task<Ok<PaginatedResponse<UserResponse>>>
        HandleGetUsers(
            [AsParameters] UserQueryRequest query,
            IUserService userService)
    {
        var results = await userService.QueryUsersAsync(query);
        return TypedResults.Ok(results);
    }

    private static async
        Task<IResult>
        HandleTestJudge(
            string resourceId,
            [AsParameters] ProblemSubmissionRequest request,
            SqidsEncoder<long> sqidsEncoder,
            IProblemSubmissionService ProblemSubmissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await ProblemSubmissionService.Send(decoded[0], request, ct);
        if (!result.IsSuccess)
        {
            return (result.Error?.Code) switch
            {
                "not_found" => TypedResults.NotFound(),
                "forbidden" => TypedResults.Forbid(),
                // incl internal
                _ => TypedResults.InternalServerError(),
            };
        }

        return TypedResults.Ok(result.Value);
    }

    private static async
        Task<IResult>
        HandleGetUtc()
    {
        return TypedResults.Ok(DateTime.UtcNow);
    }
}
