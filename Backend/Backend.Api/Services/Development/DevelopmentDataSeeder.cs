using System.Text.Json;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Assessments;
using Backend.Persistence.Entities.Content;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Users;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Development;

public static class DevelopmentDataSeeder
{
    public const string InstructorEmail = "dev.instructor@vp-lms.local";
    public const string InstructorPassword = "Instructor123!";
    public const string StudentEmail = "dev.student@vp-lms.local";
    public const string StudentPassword = "Student123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbFactory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();

        // USERS

        var instructor = await EnsureUserAsync(
            db,
            InstructorEmail,
            "John",
            "Doe (Devel)",
            InstructorPassword,
            UserRoles.Instructor);

        var student = await EnsureUserAsync(
            db,
            StudentEmail,
            "Jane",
            "Doe (Devel)",
            StudentPassword,
            UserRoles.Student);

        // COURSES

        var course = await db.Courses
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Title == "Introduction to VP-LMS");

        if (course is null)
        {
            course = new Course
            {
                InstructorId = instructor.Id,
                Title = "Introduction to VP-LMS",
                Description = "Development course for testing the authenticated student flow.",
                IsPublished = true,
                EnrollmentOpen = true
            };

            db.Courses.Add(course);
            await db.SaveChangesAsync();
        }

        // DEPARTMENTS

        List<Department> departments = [
            new Department { Name = "Department A" },
            new Department { Name = "Department B" },
            new Department { Name = "Department C" },
        ];
        List<long> departmentIds = [];

        foreach (var department in departments)
        {
            var existing = await db.Departments.FirstOrDefaultAsync(d => d.Name.Equals(department.Name));
            if (existing is null)
            {
                db.Departments.Add(department);
                await db.SaveChangesAsync();
                departmentIds.Add(department.Id);
            }
            else
            {
                departmentIds.Add(existing.Id);
            }
        }

        // TEST COURSES
        await EnsureTestCoursesAsync(db, instructor.Id, departmentIds);
        await EnsureSampleCoursesAsync(db, instructor.Id);

        // ENROLLMENTS
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

        var activity = await db.CourseActivities
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.ModuleId == module.Id && x.Type == ActivityType.Lesson && x.OrderIndex == 0);

        if (activity is null)
        {
            activity = new CourseActivity
            {
                ModuleId = module.Id,
                Type = ActivityType.Lesson,
                Title = "Welcome Lesson",
                OrderIndex = 0,
                IsPublished = true
            };
            db.CourseActivities.Add(activity);
            await db.SaveChangesAsync();
        }

        var lessonExists = await db.Lessons
            .IgnoreQueryFilters()
            .AnyAsync(x => x.ActivityId == activity.Id);

        if (!lessonExists)
        {
            db.Lessons.Add(new Lesson
            {
                ActivityId = activity.Id,
                ContentMarkdown = "# Welcome to VP-LMS\n\nThis is the first lesson in the development course."
            });
        }

        var assignmentActivity = await db.CourseActivities
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x =>
                x.ModuleId == module.Id &&
                x.Type == ActivityType.Assignment &&
                x.OrderIndex == 2);

        if (assignmentActivity is null)
        {
            assignmentActivity = new CourseActivity
            {
                ModuleId = module.Id,
                Type = ActivityType.Assignment,
                Title = "Welcome Assignment",
                OrderIndex = 2,
                IsPublished = true
            };
            db.CourseActivities.Add(assignmentActivity);
            await db.SaveChangesAsync();
        }

        var assignment = await db.Assignments
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.ActivityId == assignmentActivity.Id);

        if (assignment is null)
        {
            db.Assignments.Add(new Assignment
            {
                ActivityId = assignmentActivity.Id,
                InstructionsMD = "# Welcome Assignment\n\nWrite a short response, upload a text or PDF file, or do both to describe what you learned in the welcome lesson.",
                SubmissionType = SubmissionType.Both,
                AllowedExtensions = [".txt", ".md", ".pdf"],
                MaxFileSizeKb = 1024,
                MaxFileCount = 2,
                OpenDate = null,
                CloseDate = DateTime.UtcNow.AddMonths(6)
            });
        }

        var assessmentActivity = await db.CourseActivities
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x =>
                x.ModuleId == module.Id &&
                x.Type == ActivityType.Assessment &&
                x.OrderIndex == 1);

        if (assessmentActivity is null)
        {
            assessmentActivity = new CourseActivity
            {
                ModuleId = module.Id,
                Type = ActivityType.Assessment,
                Title = "Welcome Quiz",
                OrderIndex = 1,
                IsPublished = true
            };
            db.CourseActivities.Add(assessmentActivity);
            await db.SaveChangesAsync();
        }

        var assessment = await db.Assessments
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.ActivityId == assessmentActivity.Id);

        if (assessment is null)
        {
            assessment = new Assessment
            {
                ActivityId = assessmentActivity.Id,
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

        var codingActivity = await db.CourseActivities
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x =>
                x.ModuleId == module.Id &&
                x.Type == ActivityType.Problem &&
                x.OrderIndex == 3);

        if (codingActivity is null)
        {
            codingActivity = new CourseActivity
            {
                ModuleId = module.Id,
                Type = ActivityType.Problem,
                Title = "Add Two Numbers",
                OrderIndex = 3,
                IsPublished = true
            };
            db.CourseActivities.Add(codingActivity);
            await db.SaveChangesAsync();
        }

        var codingProblem = await db.CodingProblems
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.ActivityId == codingActivity.Id);

        if (codingProblem is null)
        {
            codingProblem = new CodingProblem
            {
                ActivityId = codingActivity.Id,
                ProblemStatementMarkdown = "# Add Two Numbers\n\nRead two integers from standard input and print their sum.",
                ConstraintsMarkdown = "- The input contains two integers separated by whitespace.\n- The absolute value of each integer is at most 1,000,000.",
                FunctionSignature = "Read from stdin and write to stdout",
                TimeLimitMs = 1000,
                MemoryLimitMb = 128,
                IsPractice = true,
                Difficulty = ProblemDifficulty.Easy
            };
            db.CodingProblems.Add(codingProblem);
            await db.SaveChangesAsync();

            db.ProblemTestCases.AddRange(
                new ProblemTestCase
                {
                    ProblemId = codingProblem.Id,
                    OrderIndex = 0,
                    Input = "2 3",
                    ExpectedOutput = "5"
                },
                new ProblemTestCase
                {
                    ProblemId = codingProblem.Id,
                    OrderIndex = 1,
                    Input = "-10 7",
                    ExpectedOutput = "-3"
                });
        }

        await db.SaveChangesAsync();
    }

    private static async Task<User> EnsureUserAsync(
        AppDbContext db,
        string email,
        string firstname,
        string lastname,
        string password,
        UserRoles role)
    {
        var user = await db.Users
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Email == email);

        if (user is not null)
        {
            user.Role = role;
            user.FirstName = firstname;
            user.LastName = lastname;
            user.IsActive = true;
            user.IsDeleted = false;
            user.DeletedAt = null;
            return user;
        }

        user = new User
        {
            Email = email,
            PasswordHash = Argon2.Hash(password),
            FirstName = firstname,
            LastName = lastname,
            Role = role,
            IsActive = true
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static async Task EnsureTestCoursesAsync(AppDbContext db, long instructorId, List<long> departmentIds)
    {
        List<Course> courses =
        [
            new Course {
                InstructorId = instructorId,
                Title = "Test course 1",
                Description = "Test empty course for development purposes, this one is published and open",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new Course {
                InstructorId = instructorId,
                Title = "Test course 2",
                Description = "Test empty course for development purposes, this one is published and closed",
                IsPublished = true,
                EnrollmentOpen = false
            },
            new Course{
                InstructorId = instructorId,
                Title = "Test course 3",
                Description = "Test empty course for development purposes, this one is unpublished and open",
                IsPublished = false,
                EnrollmentOpen = true
            }
        ];

        foreach (var course in courses.Select((x, i) => new { Value = x, Index = i }))
        {
            var departmentId = departmentIds.Skip(course.Index).FirstOrDefault();

            var existingCourse = await db.Courses.FirstOrDefaultAsync(c => c.Title == course.Value.Title);
            if (existingCourse is not null)
            {
                existingCourse.DepartmentId = departmentId;
                existingCourse.Description = course.Value.Description;
                existingCourse.IsPublished = course.Value.IsPublished;
                existingCourse.EnrollmentOpen = course.Value.EnrollmentOpen;

                db.Courses.Update(existingCourse);
            }
            else
            {
                course.Value.DepartmentId = departmentId;

                db.Courses.Add(course.Value);
            }
        }

        await db.SaveChangesAsync();
    }

    // NOTE: Should be ran after ensuring departments
    private static async Task EnsureSampleCoursesAsync(AppDbContext db, long instructorId)
    {
        var courses = new List<Course>
        {
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 1,
                Title = "Introduction to Programming",
                Description = "Learn programming fundamentals, including variables, loops, conditionals, and functions.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 1,
                Title = "Data Structures and Algorithms",
                Description = "Explore arrays, linked lists, stacks, queues, trees, graphs, and common algorithms.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 1,
                Title = "Object-Oriented Programming",
                Description = "Understand classes, objects, inheritance, polymorphism, abstraction, and encapsulation.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 2,
                Title = "Database Management Systems",
                Description = "Learn relational databases, SQL queries, normalization, indexing, and transactions.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 2,
                Title = "Web Development Fundamentals",
                Description = "Build responsive websites using HTML, CSS, JavaScript, and modern web technologies.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 2,
                Title = "Backend Development with ASP.NET Core",
                Description = "Develop RESTful APIs using C#, ASP.NET Core, Entity Framework Core, and PostgreSQL.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 3,
                Title = "Introduction to Computer Networks",
                Description = "Study network protocols, TCP/IP, routing, switching, and network security fundamentals.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 3,
                Title = "Operating Systems",
                Description = "Explore processes, threads, memory management, file systems, and concurrency.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 3,
                Title = "Software Engineering Principles",
                Description = "Learn software design, requirements analysis, testing, version control, and Agile practices.",
                IsPublished = true,
                EnrollmentOpen = false
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 1,
                Title = "Introduction to Artificial Intelligence",
                Description = "Discover AI concepts, search algorithms, knowledge representation, and intelligent agents.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 2,
                Title = "Cloud Computing Fundamentals",
                Description = "Understand cloud architecture, virtualization, containers, deployment, and cloud services.",
                IsPublished = false,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 3,
                Title = "Cybersecurity Essentials",
                Description = "Learn security principles, cryptography, authentication, common vulnerabilities, and defenses.",
                IsPublished = false,
                EnrollmentOpen = false
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 1,
                Title = "Advanced C# Programming",
                Description = "Explore generics, delegates, LINQ, asynchronous programming, and advanced C# features.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 2,
                Title = "Frontend Development with React",
                Description = "Build interactive web applications using React, TypeScript, hooks, and component-based architecture.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 2,
                Title = "RESTful API Design",
                Description = "Learn API architecture, HTTP methods, authentication, validation, and REST best practices.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 1,
                Title = "Algorithm Design and Analysis",
                Description = "Study algorithm complexity, divide-and-conquer, greedy algorithms, and dynamic programming.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 3,
                Title = "Linux System Administration",
                Description = "Learn Linux commands, shell scripting, process management, permissions, and server administration.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 2,
                Title = "Introduction to DevOps",
                Description = "Explore CI/CD pipelines, Docker, infrastructure automation, and deployment workflows.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 1,
                Title = "Machine Learning Fundamentals",
                Description = "Understand supervised learning, regression, classification, model evaluation, and feature engineering.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 3,
                Title = "Distributed Systems",
                Description = "Study distributed architectures, consistency, replication, fault tolerance, and messaging systems.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 2,
                Title = "Software Testing and Quality Assurance",
                Description = "Learn unit testing, integration testing, test automation, mocking, and software quality practices.",
                IsPublished = true,
                EnrollmentOpen = true
            },
            new()
            {
                InstructorId = instructorId,
                DepartmentId = 1,
                Title = "Introduction to Data Science",
                Description = "Explore data cleaning, exploratory analysis, visualization, statistics, and data-driven decision making.",
                IsPublished = true,
                EnrollmentOpen = true
            }
        };

        foreach (var course in courses)
        {
            var existing = await db.Courses.FirstOrDefaultAsync(c => c.Title == course.Title);
            if (existing is not null)
            {
                existing.DepartmentId = course.DepartmentId;
                existing.Description = course.Description;
                existing.IsPublished = course.IsPublished;
                existing.EnrollmentOpen = course.EnrollmentOpen;

                db.Courses.Update(existing);
            }
            else
            {
                db.Courses.Add(course);
            }
        }
        await db.SaveChangesAsync();

        // NOTE: May have to ensure that the courses are empty later.
    }
}
