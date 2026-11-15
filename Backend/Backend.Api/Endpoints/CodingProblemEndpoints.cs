using Backend.Api.Core.Types;
using Backend.Api.Services.Content;
using Backend.Persistence.Entities.Judge;
using Sqids;

namespace Backend.Api.Endpoints;

public static class CodingProblemEndpoints
{
    public static void AddCodingProblemEndpoints(
        this IEndpointRouteBuilder route)
    {
        var group =
            route.MapGroup("/api/coding-problems")
                .WithTags("Coding Problems");

        group.MapPost(
            "/module/{moduleId}",
            HandleCreate).RequireAuthorization("IsInstructor");

        group.MapGet(
            "/{activityId}",
            HandleGetById).RequireAuthorization();

        group.MapPost(
            "/{activityId}/test-cases",
            HandleAddTestCase).RequireAuthorization("IsInstructor");

        group.MapGet(
            "/{activityId}/test-cases",
            HandleGetTestCases).RequireAuthorization();

        group.MapGet(
            "/{activityId}/submissions",
            HandleGetSubmissions).RequireAuthorization("IsInstructor");

        group.MapPut(
            "/{activityId}/compiler-profile/{compilerId}",
            HandleSetCompilerProfile).RequireAuthorization("IsInstructor");

        group.MapGet(
            "/{activityId}/compiler-profile/",
            HandleGetCompilerProfiles).RequireAuthorization();
    }

    private static async Task<IResult> HandleCreate(
        string moduleId,
        CodingProblemRequest request,
        SqidsEncoder<long> sqidsEncoder,
        ICodingProblemService problemService,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(moduleId);

        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid module ID.");
        }

        var result = await problemService.CreateAsync(
            decoded[0],
            request,
            ct);

        if (result.IsSuccess)
        {
            var response = result.Value!;

            // Re-map the numeric activity ID to Sqid.
            var activity = response.ActivityInfo;

            var encodedActivityId =
                sqidsEncoder.Encode(
                    sqidsEncoder.Decode(activity.Id)[0]);

            return TypedResults.Ok(
                response with
                {
                    ActivityInfo =
                        activity with
                        {
                            Id = encodedActivityId
                        }
                });
        }

        return result.Error?.Code switch
        {
            "forbidden" =>
                TypedResults.Forbid(),

            "not_found" =>
                TypedResults.NotFound(
                    result.Error.Message),

            _ =>
                TypedResults.BadRequest(
                    result.Error?.Message ??
                    "Unable to create coding problem.")
        };
    }

    private static async Task<IResult> HandleGetById(
        string activityId,
        SqidsEncoder<long> sqidsEncoder,
        ICodingProblemService problemService,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);

        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid activity ID.");
        }

        var result =
            await problemService.GetByActivityIdAsync(
                decoded[0],
                ct);

        if (!result.IsSuccess)
        {
            return result.Error?.Code switch
            {
                "forbidden" =>
                    TypedResults.Forbid(),

                "not_found" =>
                    TypedResults.NotFound(
                        result.Error.Message),

                _ =>
                    TypedResults.BadRequest(
                        result.Error?.Message)
            };
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> HandleAddTestCase(
        string activityId,
        ProblemTestCaseRequest request,
        SqidsEncoder<long> sqidsEncoder,
        ICodingProblemService problemService,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);

        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid activity ID.");
        }

        var result = await problemService.AddTestCaseAsync(
            decoded[0],
            request,
            ct);

        if (!result.IsSuccess)
        {
            return result.Error?.Code switch
            {
                "forbidden" =>
                    TypedResults.Forbid(),

                "not_found" =>
                    TypedResults.NotFound(
                        result.Error.Message),

                _ =>
                    TypedResults.BadRequest(
                        result.Error?.Message)
            };
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> HandleGetTestCases(
        string activityId,
        SqidsEncoder<long> sqidsEncoder,
        ICodingProblemService problemService,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);

        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid activity ID.");
        }

        var result =
            await problemService.GetTestCasesAsync(
                decoded[0],
                ct);

        if (!result.IsSuccess)
        {
            return result.Error?.Code switch
            {
                "forbidden" =>
                    TypedResults.Forbid(),

                "not_found" =>
                    TypedResults.NotFound(
                        result.Error.Message),

                _ =>
                    TypedResults.BadRequest(
                        result.Error?.Message)
            };
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> HandleGetSubmissions(
        string activityId,
        SqidsEncoder<long> sqidsEncoder,
        ICodingProblemService problemService,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);
        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid activity ID.");
        }

        var result =
            await problemService.GetSubmissionsAsync(
                decoded[0],
                ct);

        if (!result.IsSuccess)
        {
            return result.Error?.Code switch
            {
                "judge_submission.not_found" =>
                    TypedResults.NotFound(
                        result.Error.Message),

                _ =>
                    TypedResults.BadRequest(
                        result.Error?.Message)
            };
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> HandleSetCompilerProfile(
        string activityId,
        string compilerId,
        ProgrammingLanguage language,
        CompilerProfileRequest request,
        SqidsEncoder<long> sqidsEncoder,
        ICodingProblemService problemService,
        CancellationToken ct)
    {
        var rDecoded = sqidsEncoder.Decode(activityId);
        var cDecoded = sqidsEncoder.Decode(compilerId);
        if (rDecoded.Count != 1 || cDecoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await problemService.SetCompilerProfileAsync(rDecoded[0], cDecoded[0], language, request, ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> HandleGetCompilerProfiles(
        string activityId,
        SqidsEncoder<long> sqidsEncoder,
        ICodingProblemService problemService,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);

        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await problemService.GetCompilerProfilesAsync(decoded[0], ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

}
