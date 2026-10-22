using Backend.Api.Core.Types;
using Backend.Api.Services.Content;
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
            "/{resourceId}",
            HandleGetById).RequireAuthorization();

        group.MapPost(
            "/{resourceId}/test-cases",
            HandleAddTestCase).RequireAuthorization("IsInstructor");

        group.MapGet(
            "/{resourceId}/test-cases",
            HandleGetTestCases).RequireAuthorization();

        group.MapPut(
            "/{resourceId}/language-profile/{compilerId}",
            HandleSetLanguageProfile).RequireAuthorization("IsInstructor");

        group.MapGet(
            "/{resourceId}/language-profile/",
            HandleGetLanguageProfiles).RequireAuthorization();
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

            // Re-map the numeric resource ID to Sqid.
            var resource = response.ResourceInfo;

            var encodedResourceId =
                sqidsEncoder.Encode(
                    sqidsEncoder.Decode(resource.Id)[0]);

            return TypedResults.Ok(
                response with
                {
                    ResourceInfo =
                        resource with
                        {
                            Id = encodedResourceId
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
        string resourceId,
        SqidsEncoder<long> sqidsEncoder,
        ICodingProblemService problemService,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);

        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid problem ID.");
        }

        var result =
            await problemService.GetByResourceIdAsync(
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
        string resourceId,
        ProblemTestCaseRequest request,
        SqidsEncoder<long> sqidsEncoder,
        ICodingProblemService problemService,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);

        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid problem ID.");
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
        string resourceId,
        SqidsEncoder<long> sqidsEncoder,
        ICodingProblemService problemService,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);

        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid problem ID.");
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

    private static async Task<IResult> HandleSetLanguageProfile(
        string resourceId,
        string compilerId,
        CompilerProfileRequest request,
        SqidsEncoder<long> sqidsEncoder,
        ICodingProblemService problemService,
        CancellationToken ct)
    {
        var rDecoded = sqidsEncoder.Decode(resourceId);
        var cDecoded = sqidsEncoder.Decode(compilerId);
        if (rDecoded.Count != 1 || cDecoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await problemService.SetLanguageProfileAsync(rDecoded[0], cDecoded[0], request, ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> HandleGetLanguageProfiles(
        string resourceId,
        SqidsEncoder<long> sqidsEncoder,
        ICodingProblemService problemService,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);

        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await problemService.GetLanguageProfilesAsync(decoded[0], ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

}
