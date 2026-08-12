using Backend.Api.Core.Entities.Assignments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Data.Configurations;

public class AssignmentSubmissionConfiguration : IEntityTypeConfiguration<AssignmentSubmission>
{
    public void Configure(EntityTypeBuilder<AssignmentSubmission> builder)
    {
        builder.ToTable("assignment_submissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SubmissionText)
            .HasColumnType("text");

        builder.HasIndex(x => new { x.AssignmentId, x.UserId })
            .HasFilter("is_deleted = false")
            .IsUnique();

        builder.HasOne(x => x.Assignment)
            .WithMany(a => a.Submissions)
            .HasForeignKey(x => x.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Grade)
            .WithOne(g => g.Submission)
            .HasForeignKey<AssignmentGrade>(g => g.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
