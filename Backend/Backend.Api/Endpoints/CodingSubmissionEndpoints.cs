using Backend.Api.Core.Types;
using Backend.Api.Services.Submissions;
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
                HandleSend)
            .RequireAuthorization("IsStudent");

        group.MapGet(
                "/{submissionId}",
                HandleGet)
            .RequireAuthorization("IsStudent");
    }

    private static async Task<IResult> HandleSend(
        string resourceId,
        JudgeRequest request,
        SqidsEncoder<long> sqidsEncoder,
        ICodingSubmissionService service,
        CancellationToken ct)
    {
        var decoded =
            sqidsEncoder.Decode(resourceId);

        if (decoded.Count != 1)
        {
            return TypedResults.BadRequest(
                "Invalid resource ID.");
        }

        var result = await service.Send(decoded[0], request, ct);

        if (!result.IsSuccess)
        {
            return result.Error?.Code switch
            {
                "forbidden" =>
                    TypedResults.Forbid(),

                "not_found" =>
                    TypedResults.NotFound(result.Error.Message),

                "internal" =>
                    TypedResults.InternalServerError(),

                _ =>
                    TypedResults.BadRequest(
                        result.Error?.Message ??
                        "Unable to create submission.")
            };
        }

        return TypedResults.Ok(result.Value!);
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

        var result = await service.Get(decoded[0], ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }
}
