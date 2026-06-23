using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Core.Types;
using Backend.Core.Common;
using Backend.Services.Courses;
using Microsoft.AspNetCore.Mvc;
using Sqids;
using Backend.Services.Users;

namespace Backend.Endpoints;

public static class EnrollmentEndpoints
{
    public static void AddEnrollmentEndpoints(this IEndpointRouteBuilder route)
    {
        var enrollment = route.MapGroup("/api/enrollment").WithTags("Enrollment");

        enrollment.MapGet("{enrollmentId}", HandleGetById).RequireAuthorization();
        enrollment.MapGet("course/{courseId}", HandleGetByCourseId).RequireAuthorization();
        enrollment.MapGet("user/{userId}", HandleGetByUserId).RequireAuthorization();
        enrollment.MapGet("user", HandleGetCurrent).RequireAuthorization();

        enrollment.MapPost("enroll/{courseId}", HandleEnroll).RequireAuthorization();
        enrollment.MapPost("unenroll/{courseId}", HandleUnenroll).RequireAuthorization();
        enrollment.MapPost("set-ta/{enrollmentId}", HandleSetTA).RequireAuthorization();
        enrollment.MapDelete("{enrollmentId}", HandleDelete).RequireAuthorization();
    }

    private static async
        Task<Results<Ok<EnrollmentDetailResponse>, BadRequest, NotFound>>
        HandleGetById(
            string enrollmentId,
            SqidsEncoder<long> sqidsEncoder,
            EnrollmentService enrollmentService)
    {
        var decoded = sqidsEncoder.Decode(enrollmentId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await enrollmentService.GetEnrollmentByIdAsync(decoded[0]);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<List<EnrollmentResponse>>, BadRequest, NotFound>>
        HandleGetByCourseId(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            EnrollmentService enrollmentService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var results = await enrollmentService.GetCourseEnrollmentsAsync(decoded[0]);
        return results is not null
            ? TypedResults.Ok(results)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<List<EnrollmentResponse>>, BadRequest, NotFound>>
        HandleGetByUserId(
            string userId,
            SqidsEncoder<long> sqidsEncoder,
            EnrollmentService enrollmentService)
    {
        var decoded = sqidsEncoder.Decode(userId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var results = await enrollmentService.GetUserEnrollmentsAsync(decoded[0]);
        return results is not null
            ? TypedResults.Ok(results)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<List<EnrollmentResponse>>, BadRequest, NotFound>>
        HandleGetCurrent(
            SqidsEncoder<long> sqidsEncoder,
            EnrollmentService enrollmentService)
    {
        var results = await enrollmentService.GetCurrentEnrollmentsAsync();
        return results is not null
            ? TypedResults.Ok(results)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<EnrollmentResponse>, BadRequest>>
        HandleEnroll(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            EnrollmentService enrollmentService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await enrollmentService.EnrollCourseAsync(decoded[0]);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest();
    }

    private static async
        Task<Results<Ok, BadRequest>>
        HandleUnenroll(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            EnrollmentService enrollmentService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await enrollmentService.UnenrollCourseAsync(decoded[0]);
        return result
            ? TypedResults.Ok()
            : TypedResults.BadRequest();
    }

    private static async
        Task<Results<Ok<EnrollmentResponse>, BadRequest>>
        HandleSetTA(
            string enrollmentId,
            SqidsEncoder<long> sqidsEncoder,
            EnrollmentService enrollmentService,
            [FromQuery] bool enable = true)
    {
        var decoded = sqidsEncoder.Decode(enrollmentId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await enrollmentService.SetTAAsync(decoded[0], enable);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest();
    }

    private static async
        Task<Results<Ok, BadRequest, NotFound>>
        HandleDelete(
            string enrollmentId,
            SqidsEncoder<long> sqidsEncoder,
            EnrollmentService enrollmentService)
    {
        var decoded = sqidsEncoder.Decode(enrollmentId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await enrollmentService.RemoveEnrollmentAsync(decoded[0]);
        return result
            ? TypedResults.Ok()
            : TypedResults.BadRequest();
    }
}
