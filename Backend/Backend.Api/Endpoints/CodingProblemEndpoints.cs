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
            "/{resourceId}/language-profile/",
            HandleSetLanguageProfile).RequireAuthorization("IsInstructor");

        group.MapGet(
            "/{resourceId}/language-profile/",
            HandleGetLanguageProfiles).RequireAuthorization();
    }

    private static async Task<IResult> HandleCreate(
        string moduleId,
        CodingProblemRequest request,
        SqidsEncoder<long> sqidsEncoder,
        CodingProblemService service,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(moduleId);

        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid module ID.");
        }

        var result = await service.CreateAsync(
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
        CodingProblemService service,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);

        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid problem ID.");
        }

        var result =
            await service.GetByResourceIdAsync(
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
        CodingProblemService service,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);

        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid problem ID.");
        }

        var result = await service.AddTestCaseAsync(
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
        CodingProblemService service,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);

        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid problem ID.");
        }

        var result =
            await service.GetTestCasesAsync(
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
        LanguageProfileDto request,
        SqidsEncoder<long> sqidsEncoder,
        CodingProblemService service,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);

        if (decoded.Count != 1)
            return TypedResults.BadRequest();
    }
}
