using Backend.Models.Submissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

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

        builder.HasOne(x => x.Assessment)
            .WithMany(a => a.Attempts)
            .HasForeignKey(x => x.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Responses)
            .WithOne(r => r.Attempt)
            .HasForeignKey(r => r.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
