using Backend.Persistence.Entities.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Persistence.Data.Configurations;

public class AssessmentAttemptConfiguration : IEntityTypeConfiguration<AssessmentAttempt>
{
    public void Configure(EntityTypeBuilder<AssessmentAttempt> builder)
    {
        builder.ToTable("assessment_attempts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StartedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.TotalScore)
            .HasColumnType("decimal(5,2)");

        builder.HasIndex(x => new { x.AssessmentId, x.StudentId, x.AttemptNumber }).IsUnique();

        builder.HasOne(x => x.Assessment)
            .WithMany(a => a.Attempts)
            .HasForeignKey(x => x.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasMany(x => x.Questions)
            .WithOne(r => r.Attempt)
            .HasForeignKey(r => r.AttemptId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
