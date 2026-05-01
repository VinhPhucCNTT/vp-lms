using Backend.Models.Common;
using Backend.Models.Courses;
using Backend.Models.Resources;
using Backend.Models.Submissions;
using Backend.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<ModuleResource> ModuleResources => Set<ModuleResource>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Coding> Codings => Set<Coding>();
    public DbSet<CodingTestCase> TestCases => Set<CodingTestCase>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<AssessmentQuestion> AssessmentQuestions => Set<AssessmentQuestion>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<TAPermissions> TAPermissions => Set<TAPermissions>();
    public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();
    public DbSet<AssignmentGrade> AssignmentGrades => Set<AssignmentGrade>();
    public DbSet<CodingSubmission> CodingSubmissions => Set<CodingSubmission>();
    public DbSet<CodingTestResult> CodingTestResults => Set<CodingTestResult>();
    public DbSet<AssessmentAttempt> AssessmentAttempts => Set<AssessmentAttempt>();
    public DbSet<AssessmentResponse> AssessmentResponses => Set<AssessmentResponse>();
    public DbSet<ResourceComment> ResourceComments => Set<ResourceComment>();
    public DbSet<ResourceProgress> ResourceProgress => Set<ResourceProgress>();
    public DbSet<CodeExecutionLog> CodeExecutionLogs => Set<CodeExecutionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Additional configurations that can't be in separate files

        // Configure indexes for commonly queried fields
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false"); // Only enforce uniqueness for non-deleted users

        modelBuilder.Entity<Course>()
            .HasIndex(c => c.CreatorId)
            .HasFilter("\"IsDeleted\" = false");

        modelBuilder.Entity<ModuleResource>()
            .HasIndex(r => r.ModuleId)
            .HasFilter("\"IsDeleted\" = false");

        // Composite indexes for common queries
        modelBuilder.Entity<CodingSubmission>()
            .HasIndex(s => new { s.ChallengeId, s.UserId });

        modelBuilder.Entity<AssessmentAttempt>()
            .HasIndex(a => new { a.AssessmentId, a.UserId });

        modelBuilder.Entity<ResourceProgress>()
            .HasIndex(p => new { p.UserId, p.IsCompleted });
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
