using Backend.Api.Core.Types;
using Backend.Api.Services.Dashboard;
namespace Backend.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void AddDashboardEndpoints(this IEndpointRouteBuilder route)
    {
        var dashboard = route.MapGroup("/api/dashboard").WithTags("Dashboard");

        dashboard.MapGet("student", HandleGetStudent).RequireAuthorization("IsStudent");

        dashboard.MapGet("instructor", HandleGetInstructor).RequireAuthorization("IsInstructor");
    }

    private static async
        Task<IResult>
        HandleGetStudent(
            IDashboardService dashboardService,
            CancellationToken ct)
    {
        var results = await dashboardService.GetStudentDashboardAsync(ct);
        return TypedResults.Ok(results);
    }

    private static async
        Task<IResult>
        HandleGetInstructor(
            IDashboardService dashboardService,
            CancellationToken ct)
    {
        var results = await dashboardService.GetInstructorDashboardAsync(ct);
        return TypedResults.Ok(results);
    }
}
