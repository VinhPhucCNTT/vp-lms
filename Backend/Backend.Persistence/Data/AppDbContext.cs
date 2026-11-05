using Backend.Persistence.Common;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Content;
using Backend.Persistence.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Backend.Persistence.Entities.Assignments;
using Backend.Persistence.Entities.Judge;
using Backend.Persistence.Entities.Assessments;
using Backend.Persistence.Entities.Learning;
using System.Linq.Expressions;

namespace Backend.Persistence.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<FileAsset> FileAssets => Set<FileAsset>();

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseModule> CourseModules => Set<CourseModule>();

    public DbSet<CourseResource> CourseResources => Set<CourseResource>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ResourceProgress> ResourceProgress => Set<ResourceProgress>();
    public DbSet<RecentCourse> RecentCourses => Set<RecentCourse>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();
    public DbSet<AssignmentGrade> AssignmentGrades => Set<AssignmentGrade>();
    public DbSet<AssignmentFile> AssignmentFiles => Set<AssignmentFile>();

    public DbSet<CodingProblem> CodingProblems => Set<CodingProblem>();
    public DbSet<ProblemTestCase> ProblemTestCases => Set<ProblemTestCase>();
    public DbSet<ProblemSubmission> ProblemSubmissions => Set<ProblemSubmission>();
    public DbSet<ProblemTestResult> ProblemTestResults => Set<ProblemTestResult>();
    public DbSet<CodeExecutionLog> CodeExecutionLogs => Set<CodeExecutionLog>();
    public DbSet<Compiler> Compilers => Set<Compiler>();
    public DbSet<CompilerProfile> CompilerProfiles => Set<CompilerProfile>();

    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionBank> QuestionBanks => Set<QuestionBank>();
    public DbSet<AttemptQuestion> AttemptQuestions => Set<AttemptQuestion>();
    public DbSet<AssessmentQuestion> AssessmentQuestions => Set<AssessmentQuestion>();
    public DbSet<AssessmentAttempt> AssessmentAttempts => Set<AssessmentAttempt>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<TAPermissions> TAPermissions => Set<TAPermissions>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ConfigureSoftDelete(modelBuilder);
    }

    private static void ConfigureSoftDelete(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType)) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isDeleted = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var condition = Expression.Equal(isDeleted, Expression.Constant(false));
            var lambda = Expression.Lambda(condition, parameter);

            builder.Entity(entityType.ClrType).HasQueryFilter("SoftDelete", lambda);

            foreach (var index in entityType.GetIndexes().Where(i => i.IsUnique && i.GetFilter() is null))
            {
                index.SetFilter("is_deleted = false");
            }
        }
    }

    // Old method
    // private static void ConfigureSoftDelete(ModelBuilder builder)
    // {
    //     // Use anonymous name instead
    //     // builder.Entity<User>()
    //     //     .HasQueryFilter(u => !u.IsDeleted);
    //
    //     builder.Entity<Course>()
    //         .HasQueryFilter(x => !x.IsDeleted && !x.Creator.IsDeleted);
    //     builder.Entity<Enrollment>()
    //         .HasQueryFilter(x => !x.IsDeleted && !x.Course.IsDeleted);
    //     builder.Entity<CourseModule>()
    //         .HasQueryFilter(x => !x.IsDeleted && !x.Course.IsDeleted);
    //     builder.Entity<CourseResource>()
    //         .HasQueryFilter(x => !x.IsDeleted && !x.Module.IsDeleted);
    //
    //     builder.Entity<Lesson>()
    //         .HasQueryFilter(x => !x.IsDeleted && !x.Resource.IsDeleted);
    //
    //     builder.Entity<Assignment>()
    //         .HasQueryFilter(x => !x.IsDeleted && !x.Resource.IsDeleted);
    //
    //     builder.Entity<Assessment>()
    //         .HasQueryFilter(x => !x.IsDeleted && !x.Resource.IsDeleted);
    //     builder.Entity<AssessmentQuestion>()
    //         .HasQueryFilter(x => !x.Assessment.IsDeleted);
    //     builder.Entity<AttemptQuestion>()
    //         .HasQueryFilter(x => !x.AssessmentQuestion.Assessment.IsDeleted);
    //
    //     builder.Entity<CodingProblem>()
    //         .HasQueryFilter(x => !x.IsDeleted && !x.Resource.IsDeleted);
    //     builder.Entity<CompilerProfile>()
    //         .HasQueryFilter(x => !x.Problem.IsDeleted);
    //     builder.Entity<ProblemSubmission>()
    //         .HasQueryFilter(x => !x.Problem.IsDeleted);
    //
    //     builder.Entity<QuestionBank>()
    //         .HasQueryFilter(x => !x.IsDeleted);
    //     builder.Entity<Question>()
    //         .HasQueryFilter(x => !x.IsDeleted);
    //
    //     builder.Entity<AssignmentSubmission>()
    //         .HasQueryFilter(x => !x.IsDeleted && !x.Assignment.IsDeleted);
    //     builder.Entity<AssignmentFile>()
    //         .HasQueryFilter(x => !x.Submission.IsDeleted);
    //
    //     // builder.Entity<AttemptAnswer>()
    //     //     .HasQueryFilter(r => !r.Question.IsDeleted);
    //
    //     // Use anonymous name instead
    //     // builder.Entity<ResourceComment>()
    //     //     .HasQueryFilter(c => !c.IsDeleted && !c.Resource.IsDeleted);
    // }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateEntities();
        HandleSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateEntities();
        HandleSoftDelete();
        return base.SaveChanges();
    }

    private void UpdateEntities()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            var now = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
                entity.CreatedAt = now;

            entity.UpdatedAt = now;
        }
    }

    private void HandleSoftDelete()
    {
        var deletedEntries = ChangeTracker.Entries()
            .Where(e => e.Entity is ISoftDeletable && e.State == EntityState.Deleted);

        foreach (var entry in deletedEntries)
        {
            entry.State = EntityState.Modified;
            var entity = (ISoftDeletable)entry.Entity;
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;

            // Mark navigation properties as unchanged to avoid cascade issues
            foreach (var navigation in entry.Navigations)
            {
                if (!navigation.IsModified)
                    navigation.IsModified = false;
            }
        }
    }
}
