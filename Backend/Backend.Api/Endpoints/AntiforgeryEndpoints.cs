using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Api.Endpoints;

public static class AntiforgeryEndpoints
{
    public static void AddAntiforgeryEndpoints(this IEndpointRouteBuilder route)
    {
        route.MapGet("/api/antiforgery/token", HandleGetToken)
            .RequireAuthorization();
    }

    private static Ok<AntiforgeryTokenResponse> HandleGetToken(
        IAntiforgery antiforgery,
        HttpContext context)
    {
        var tokens = antiforgery.GetAndStoreTokens(context);
        return TypedResults.Ok(new AntiforgeryTokenResponse(tokens.RequestToken));
    }

    private sealed record AntiforgeryTokenResponse(string? Token);
}
