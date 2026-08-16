using Backend.Persistence.Data;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Content;
using Backend.Persistence.Entities.Assessments;
using Backend.Persistence.Entities.Users;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend.Api.Services.Development;

public static class DevelopmentDataSeeder
{
    public const string InstructorEmail = "dev.instructor@vp-lms.local";
    public const string InstructorPassword = "Instructor123!";
    public const string StudentEmail = "dev.student@vp-lms.local";
    public const string StudentPassword = "Student123!";
    public const string CourseCode = "DEV-INTRO";

    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbFactory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();

        var instructor = await EnsureUserAsync(
            db,
            InstructorEmail,
            "dev-instructor",
            "Development Instructor",
            InstructorPassword,
            UserRoles.Instructor);

        var student = await EnsureUserAsync(
            db,
            StudentEmail,
            "dev-student",
            "Development Student",
            StudentPassword,
            UserRoles.Student);

        var course = await db.Courses
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Code == CourseCode);

        if (course is null)
        {
            course = new Course
            {
                CreatorId = instructor.Id,
                Code = CourseCode,
                Title = "Introduction to VP-LMS",
                Description = "Development course for testing the authenticated student flow.",
                IsPublished = true,
                EnrollmentOpen = true
            };

            db.Courses.Add(course);
            await db.SaveChangesAsync();
        }

        var enrollmentExists = await db.Enrollments
            .IgnoreQueryFilters()
            .AnyAsync(x => x.CourseId == course.Id && x.UserId == student.Id);

        if (!enrollmentExists)
        {
            db.Enrollments.Add(new Enrollment
            {
                CourseId = course.Id,
                UserId = student.Id,
                Role = EnrollmentRole.Student
            });
        }

        var module = await db.CourseModules
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.CourseId == course.Id && x.OrderIndex == 0);

        if (module is null)
        {
            module = new CourseModule
            {
                CourseId = course.Id,
                Title = "Welcome to the Course",
                Description = "A starter module for the development vertical slice.",
                OrderIndex = 0,
                IsPublished = true
            };
            db.CourseModules.Add(module);
            await db.SaveChangesAsync();
        }

        var resource = await db.CourseResources
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.ModuleId == module.Id && x.Type == ResourceType.Lesson && x.OrderIndex == 0);

        if (resource is null)
        {
            resource = new CourseResource
            {
                ModuleId = module.Id,
                Type = ResourceType.Lesson,
                Title = "Welcome Lesson",
                OrderIndex = 0,
                IsPublished = true
            };
            db.CourseResources.Add(resource);
            await db.SaveChangesAsync();
        }

        var lessonExists = await db.Lessons
            .IgnoreQueryFilters()
            .AnyAsync(x => x.ResourceId == resource.Id);

        if (!lessonExists)
        {
            db.Lessons.Add(new Lesson
            {
                ResourceId = resource.Id,
                ContentMarkdown = "# Welcome to VP-LMS\n\nThis is the first lesson in the development course."
            });
        }

        var assignmentResource = await db.CourseResources
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x =>
                x.ModuleId == module.Id &&
                x.Type == ResourceType.Assignment &&
                x.OrderIndex == 2);

        if (assignmentResource is null)
        {
            assignmentResource = new CourseResource
            {
                ModuleId = module.Id,
                Type = ResourceType.Assignment,
                Title = "Welcome Assignment",
                OrderIndex = 2,
                IsPublished = true
            };
            db.CourseResources.Add(assignmentResource);
            await db.SaveChangesAsync();
        }

        var assignment = await db.Assignments
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.ResourceId == assignmentResource.Id);

        if (assignment is null)
        {
            db.Assignments.Add(new Assignment
            {
                ResourceId = assignmentResource.Id,
                InstructionsMD = "# Welcome Assignment\n\nUpload a short text or PDF file describing what you learned in the welcome lesson.",
                SubmissionType = SubmissionType.File,
                AllowedExtensions = [".txt", ".md", ".pdf"],
                MaxFileSizeKb = 1024,
                MaxFileCount = 2,
                OpenDate = null,
                CloseDate = DateTime.UtcNow.AddMonths(6)
            });
        }

        var assessmentResource = await db.CourseResources
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x =>
                x.ModuleId == module.Id &&
                x.Type == ResourceType.Assessment &&
                x.OrderIndex == 1);

        if (assessmentResource is null)
        {
            assessmentResource = new CourseResource
            {
                ModuleId = module.Id,
                Type = ResourceType.Assessment,
                Title = "Welcome Quiz",
                OrderIndex = 1,
                IsPublished = true
            };
            db.CourseResources.Add(assessmentResource);
            await db.SaveChangesAsync();
        }

        var assessment = await db.Assessments
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.ResourceId == assessmentResource.Id);

        if (assessment is null)
        {
            assessment = new Assessment
            {
                ResourceId = assessmentResource.Id,
                Description = "A short development assessment used to verify the assessment start flow.",
                TimeLimitMinutes = 10,
                MaxAttempts = 1,
                ShowResults = true
            };
            db.Assessments.Add(assessment);
            await db.SaveChangesAsync();
        }

        var questionExists = await db.AssessmentQuestions
            .AnyAsync(x => x.AssessmentId == assessment.Id && x.OrderIndex == 0);

        if (!questionExists)
        {
            db.AssessmentQuestions.Add(new AssessmentQuestion
            {
                AssessmentId = assessment.Id,
                QuestionType = QuestionType.TrueFalse,
                Text = "VP-LMS development assessments are backed by the shared PostgreSQL database.",
                QuestionData = JsonDocument.Parse("{\"correctAnswer\":true}"),
                OrderIndex = 0,
                Points = 1
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task<User> EnsureUserAsync(
        AppDbContext db,
        string email,
        string username,
        string fullname,
        string password,
        UserRoles role)
    {
        var user = await db.Users
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Email == email);

        if (user is not null)
        {
            user.Role = role;
            user.IsActive = true;
            user.IsDeleted = false;
            user.DeletedAt = null;
            return user;
        }

        user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = Argon2.Hash(password),
            Fullname = fullname,
            Role = role,
            IsActive = true
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }
}
