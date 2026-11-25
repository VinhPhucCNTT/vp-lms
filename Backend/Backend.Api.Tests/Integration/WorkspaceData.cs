using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Users;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Tests.Integration;

internal sealed class WorkspaceData(WorkspaceApiFactory api) : IAsyncDisposable
{
    private const string Password = "Workspace-test-login-only!";
    private readonly string marker = $"workspace-it-{Guid.NewGuid():N}";
    private long[] userIds = [];

    internal User Owner { get; private set; } = null!;
    internal User Student { get; private set; } = null!;
    internal User Unenrolled { get; private set; } = null!;
    internal User OtherInstructor { get; private set; } = null!;
    internal Course Course { get; private set; } = null!;
    internal CourseModule MainModule { get; private set; } = null!;
    internal CourseModule EmptyModule { get; private set; } = null!;
    internal CourseModule DraftModule { get; private set; } = null!;
    internal CourseModule DeletedModule { get; private set; } = null!;
    internal CourseActivity FirstActivity { get; private set; } = null!;
    internal CourseActivity SecondActivity { get; private set; } = null!;
    internal string CourseUrl => $"/api/course/{api.Encode(Course.Id)}";
    internal string ModulesUrl => $"{CourseUrl}/modules";
    internal string ActivitiesUrl(CourseModule module) => $"/api/module/{api.Encode(module.Id)}/activities";

    internal async Task SeedAsync()
    {
        await using var db = await api.OpenDbAsync();
        var hash = Argon2.Hash(Password);
        Owner = MakeUser("Owner", UserRoles.Instructor, hash);
        Student = MakeUser("Student", UserRoles.Student, hash);
        Unenrolled = MakeUser("Unenrolled", UserRoles.Student, hash);
        OtherInstructor = MakeUser("Other", UserRoles.Instructor, hash);
        Course = new Course
        {
            Title = marker, Description = "Integration course", Instructor = Owner,
            IsPublished = true, EnrollmentOpen = false
        };
        Course.Enrollments.Add(new Enrollment { User = Student, Role = EnrollmentRole.Student });

        MainModule = AddModule("Main", 20);
        EmptyModule = AddModule("Empty", 10);
        DraftModule = AddModule("Draft", 5, published: false);
        DeletedModule = AddModule("Deleted", 1, deleted: true);
        SecondActivity = AddActivity(MainModule, "Assignment", 20, ActivityType.Assignment);
        FirstActivity = AddActivity(MainModule, "Lesson", 10, ActivityType.Lesson);
        AddActivity(MainModule, "Draft", 5, ActivityType.Assessment, published: false);
        AddActivity(MainModule, "Deleted", 1, ActivityType.Problem, deleted: true);
        AddActivity(DraftModule, "Published child", 0, ActivityType.Lesson);
        AddActivity(DeletedModule, "Published child", 0, ActivityType.Lesson);

        db.Users.AddRange(Owner, Student, Unenrolled, OtherInstructor);
        db.Courses.Add(Course);
        await db.SaveChangesAsync();
        userIds = [Owner.Id, Student.Id, Unenrolled.Id, OtherInstructor.Id];
    }

    internal Task<HttpClient> LoginAsync(User user) => api.LoginAsync(user.Email, Password);

    private User MakeUser(string name, UserRoles role, string hash) => new()
    {
        Email = $"{marker}-{name.ToLowerInvariant()}@example.test",
        FirstName = name, LastName = "Integration", Role = role, PasswordHash = hash
    };

    private CourseModule AddModule(string title, int order, bool published = true, bool deleted = false)
    {
        var module = new CourseModule { Title = title, OrderIndex = order, IsPublished = published, IsDeleted = deleted };
        Course.Modules.Add(module);
        return module;
    }

    private static CourseActivity AddActivity(CourseModule module, string title, int order, ActivityType type,
        bool published = true, bool deleted = false)
    {
        var activity = new CourseActivity
        {
            Title = title, OrderIndex = order, Type = type, IsPublished = published, IsDeleted = deleted
        };
        module.Activities.Add(activity);
        return activity;
    }

    public async ValueTask DisposeAsync()
    {
        if (userIds.Length == 0)
            return;

        await using var db = await api.OpenDbAsync();
        await using var tx = await db.Database.BeginTransactionAsync();
        var courseId = Course.Id;
        var modules = db.CourseModules.IgnoreQueryFilters().Where(m => m.CourseId == courseId);
        var moduleIds = modules.Select(m => m.Id);
        // Bypass soft delete and remove children before their parents, including hidden rows.
        await db.RecentCourses.IgnoreQueryFilters().Where(r => r.CourseId == courseId).ExecuteDeleteAsync();
        await db.Enrollments.IgnoreQueryFilters().Where(e => e.CourseId == courseId).ExecuteDeleteAsync();
        await db.CourseActivities.IgnoreQueryFilters().Where(a => moduleIds.Contains(a.ModuleId)).ExecuteDeleteAsync();
        await modules.ExecuteDeleteAsync();
        await db.Courses.IgnoreQueryFilters().Where(c => c.Id == courseId && c.Title == marker).ExecuteDeleteAsync();
        await db.Users.Where(u => userIds.Contains(u.Id)).ExecuteDeleteAsync();
        await tx.CommitAsync();
    }
}
