using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Core.Types;
using Backend.Core.Common;
using Backend.Services.Courses;
using Microsoft.AspNetCore.Mvc;
using Sqids;

namespace Backend.Endpoints;

public static class CourseEndpoints
{
    public static void AddCourseEndpoints(this IEndpointRouteBuilder route)
    {
        var course = route.MapGroup("/api/course");

        course.MapGet("{courseSqids}", HandleGetById);
        course.MapGet("query", HandleQuery);
    }

    private static async
        Task<Results<Ok<CourseDetailResponse>, BadRequest>>
        HandleGetById([FromRoute] string courseSqids, SqidsEncoder<long> sqidsEncoder, CourseService courseService)
        {
            var courseId = sqidsEncoder.Decode(courseSqids);
            var result = await courseService.GetCourseByIdAsync(courseId);
        }

    private static async
        Task<Ok<QueryResponse<CourseResponse>>>
        HandleQuery([AsParameters] CourseRequest query, CourseService courseService)
        {
            var result = await courseService.QueryCoursesAsync(query);
            return TypedResults.Ok(result);
        }
}
