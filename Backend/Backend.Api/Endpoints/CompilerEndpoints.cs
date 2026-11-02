using Backend.Api.Core.Types;
using Backend.Api.Services.Judge;
using Sqids;

namespace Backend.Api.Endpoints;

public static class CompilerEndpoints
{
    public static void AddCompilerEndpoints(this IEndpointRouteBuilder route)
    {
        var compiler = route.MapGroup("/api/compiler").RequireAuthorization().WithTags("Compilers");

        compiler.MapGet("{compilerId}", HandleGet);

        compiler.MapGet("query", HandleQuery);

        compiler.MapPost("", HandleCreate);

        compiler.MapPut("{compilerId}", HandleUpdate);

        compiler.MapDelete("{compilerId}", HandleDelete);
    }

    private static async
        Task<IResult>
        HandleGet(
            string compilerId,
            ICompilerService compilerService,
            SqidsEncoder<long> sqidsEncoder,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(compilerId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await compilerService.GetAsync(decoded[0], ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<IResult>
        HandleQuery(
            [AsParameters] CompilerQueryRequest query,
            ICompilerService compilerService,
            CancellationToken ct)
    {
        return TypedResults.Ok(await compilerService.QueryAsync(query, ct));
    }

    private static async
        Task<IResult>
        HandleCreate(
            CompilerRequest request,
            ICompilerService compilerService,
            CancellationToken ct)
    {
        var result = await compilerService.CreateAsync(request, ct);
        if (!result.IsSuccess)
        {
            switch (result.Error?.Code)
            {
                case "compiler.Internal":
                    return TypedResults.InternalServerError();
                case "compiler.NotFound":
                    return TypedResults.NotFound();
            }
        }
        return TypedResults.Ok(result.Value);
    }

    private static async
        Task<IResult>
        HandleUpdate(
            string compilerId,
            CompilerRequest request,
            ICompilerService compilerService,
            SqidsEncoder<long> sqidsEncoder,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(compilerId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await compilerService.UpdateAsync(decoded[0], request, ct);
        if (!result.IsSuccess)
        {
            switch (result.Error?.Code)
            {
                case "compiler.Internal":
                    return TypedResults.InternalServerError();
                case "compiler.NotFound":
                    return TypedResults.NotFound();
            }
        }
        return TypedResults.Ok(result.Value);
    }

    private static async
        Task<IResult>
        HandleDelete(
            string compilerId,
            ICompilerService compilerService,
            SqidsEncoder<long> sqidsEncoder,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(compilerId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await compilerService.DeleteAsync(decoded[0], ct);
        if (!result.IsSuccess)
        {
            switch (result.Error?.Code)
            {
                case "compiler.Internal":
                    return TypedResults.InternalServerError();
                case "compiler.NotFound":
                    return TypedResults.NotFound();
            }
        }
        return TypedResults.Ok(result.Value);
    }
}
