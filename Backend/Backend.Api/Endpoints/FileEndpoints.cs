using Backend.Api.Services.Content;
using Sqids;

namespace Backend.Api.Endpoints;

public static class FileEndpoints
{
    public static void AddFileEndpoints(this IEndpointRouteBuilder route)
    {
        var file = route.MapGroup("/api/file").WithTags("Files");

        file.MapGet("{fileId}", HandleGet);
    }

    private static async Task<IResult> HandleGet(
            string fileId,
            SqidsEncoder<long> sqidsEncoder,
            IFileService fileService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(fileId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await fileService.GetFileAsync(decoded[0], ct);
        if (result is null)
            return TypedResults.NotFound();

        return result;
    }
}
