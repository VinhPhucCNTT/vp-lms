using Backend.Core.Common.Models;
using Backend.Core.Entities.Courses;
using Backend.Core.Entities.Resources;
using Backend.Core.Entities.Submissions;
using Backend.Core.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseModule> Modules => Set<CourseModule>();
    public DbSet<ModuleResource> ModuleResources => Set<ModuleResource>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Problem> Problems => Set<Problem>();
    public DbSet<ProblemTestCase> TestCases => Set<ProblemTestCase>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<AssessmentQuestion> AssessmentQuestions => Set<AssessmentQuestion>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<TAPermissions> TAPermissions => Set<TAPermissions>();
    public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();
    public DbSet<AssignmentGrade> AssignmentGrades => Set<AssignmentGrade>();
    public DbSet<ProblemSubmission> ProblemSubmissions => Set<ProblemSubmission>();
    public DbSet<ProblemTestResult> ProblemTestResults => Set<ProblemTestResult>();
    public DbSet<AssessmentAttempt> AssessmentAttempts => Set<AssessmentAttempt>();
    public DbSet<AssessmentResponse> AssessmentResponses => Set<AssessmentResponse>();
    public DbSet<ResourceComment> ResourceComments => Set<ResourceComment>();
    public DbSet<ResourceProgress> ResourceProgress => Set<ResourceProgress>();
    public DbSet<CodeExecutionLog> CodeExecutionLogs => Set<CodeExecutionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ConfigureSoftDelete(modelBuilder);
    }

    private static void ConfigureSoftDelete(ModelBuilder builder)
    {
        // Use anonymous name instead
        // builder.Entity<User>()
        //     .HasQueryFilter(u => !u.IsDeleted);

        builder.Entity<Course>()
            .HasQueryFilter(c => !c.IsDeleted && !c.Creator.IsDeleted);
        builder.Entity<Enrollment>()
            .HasQueryFilter(e => !e.IsDeleted && !e.Course.IsDeleted);
        builder.Entity<CourseModule>()
            .HasQueryFilter(m => !m.IsDeleted && !m.Course.IsDeleted);
        builder.Entity<ModuleResource>()
            .HasQueryFilter(r => !r.IsDeleted && !r.Module.IsDeleted);

        builder.Entity<Lesson>()
            .HasQueryFilter(l => !l.IsDeleted && !l.Resource.IsDeleted);
        builder.Entity<Assignment>()
            .HasQueryFilter(a => !a.IsDeleted && !a.Resource.IsDeleted);
        builder.Entity<Assessment>()
            .HasQueryFilter(a => !a.IsDeleted && !a.Resource.IsDeleted);
        builder.Entity<Problem>()
            .HasQueryFilter(j => !j.IsDeleted && !j.Resource.IsDeleted);

        builder.Entity<AssignmentSubmission>()
            .HasQueryFilter(s => !s.IsDeleted && !s.Assignment.IsDeleted);

        builder.Entity<AssessmentQuestion>()
            .HasQueryFilter(q => !q.IsDeleted && !q.Assessment.IsDeleted);
        builder.Entity<AssessmentResponse>()
            .HasQueryFilter(r => !r.Question.IsDeleted);

        // Use anonymous name instead
        // builder.Entity<ResourceComment>()
        //     .HasQueryFilter(c => !c.IsDeleted && !c.Resource.IsDeleted);
    }


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
                if (!navigation.IsModified)
                    navigation.IsModified = false;
        }
    }
}
