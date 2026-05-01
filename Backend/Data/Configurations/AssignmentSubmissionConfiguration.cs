using Backend.Models.Submissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class AssignmentSubmissionConfiguration : IEntityTypeConfiguration<AssignmentSubmission>
{
    public void Configure(EntityTypeBuilder<AssignmentSubmission> builder)
    {
        builder.ToTable("assignment_submissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SubmissionText)
            .HasColumnType("text");

        builder.Property(x => x.FileUrl)
            .HasMaxLength(500);

        builder.Property(x => x.FileName)
            .HasMaxLength(255);

        builder.HasIndex(x => new { x.AssignmentId, x.UserId })
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
