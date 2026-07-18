using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Core.Types;
using Sqids;

namespace Backend.Endpoints;

public static class LessonEndpoints
{
    public static void AddLessonEndpoints(this IEndpointRouteBuilder route)
    {
        var lesson = route.MapGroup("/api/lesson");

        lesson.MapGet("{resourceId}", HandleGetById).RequireAuthorization();
        lesson.MapPost("{moduleId}", HandleCreate).RequireAuthorization();
        lesson.MapPut("{resourceId}", HandleUpdate).RequireAuthorization();
    }

    private static async
        Task<Results<Ok<LessonSetResponse>, BadRequest, NotFound>>
        HandleGetById(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
    }

}

