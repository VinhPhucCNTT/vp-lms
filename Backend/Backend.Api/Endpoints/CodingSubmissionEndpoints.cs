using Backend.Api.Core.Types;
using Backend.Api.Services.Judge;
using Backend.Persistence.Common;
using Backend.Persistence.Entities.Judge;
using Sqids;

namespace Backend.Api.Endpoints;

public static class CodingSubmissionEndpoints
{
    public static void AddCodingSubmissionEndpoints(
        this IEndpointRouteBuilder route)
    {
        var group =
            route.MapGroup("/api/coding-submissions")
                .WithTags("Coding Submissions");

        group.MapPost(
                "/{resourceId}",
                HandleCreate)
            .RequireAuthorization("IsStudent");

        group.MapGet(
                "/{submissionId}",
                HandleGet)
            .RequireAuthorization("IsStudent");
    }

    private static async Task<IResult> HandleCreate(
        string resourceId,
        CreateCodingSubmissionRequest request,
        SqidsEncoder<long> sqidsEncoder,
        CodingSubmissionService service,
        CancellationToken ct)
    {
        var decoded =
            sqidsEncoder.Decode(resourceId);

        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid problem ID.");
        }

        var result =
            await service.CreateAsync(
                decoded[0],
                request.SourceCode,
                (Core.Common.ProgrammingLanguage)request.Language,
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
                        result.Error?.Message ??
                        "Unable to create submission.")
            };
        }

        var submissionId =
            sqidsEncoder.Encode(result.Value);

        return TypedResults.Ok(
            new CodingSubmissionResponse(
                submissionId,
                SubmissionStatus.Pending.ToString(),
                null,
                null,
                null,
                null));
    }

    private static async Task<IResult> HandleGet(
        string submissionId,
        SqidsEncoder<long> sqidsEncoder,
        CodingSubmissionService service,
        CancellationToken ct)
    {
        var decoded =
            sqidsEncoder.Decode(submissionId);

        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid submission ID.");
        }

        var result =
            await service.GetResultAsync(
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

        var value = result.Value!;

        return TypedResults.Ok(
            new CodingSubmissionResponse(
                submissionId,
                value.Status.ToString(),
                value.ExecutionTimeMs,
                value.MemoryUsedKb,
                value.PassedTestCases,
                value.TotalTestCases));
    }
}

public sealed record CreateCodingSubmissionRequest(
    string SourceCode,
    ProgrammingLanguage Language);

public sealed record CodingSubmissionResponse(
    string Id,
    string Status,
    int? ExecutionTimeMs,
    int? MemoryUsedKb,
    int? PassedTestCases,
    int? TotalTestCases);
