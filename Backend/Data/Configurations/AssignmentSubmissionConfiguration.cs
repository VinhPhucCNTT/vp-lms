using Backend.Models.Assignments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class AssignmentSubmissionConfiguration 
    : IEntityTypeConfiguration<AssignmentSubmission>
{
    public void Configure(EntityTypeBuilder<AssignmentSubmission> builder)
    {
        builder.ToTable("AssignmentSubmissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SubmissionText)
            .HasMaxLength(5000);

        builder.Property(x => x.FileUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Grade)
            .HasPrecision(5, 2);

        builder.Property(x => x.Feedback)
            .HasMaxLength(2000);

        builder.HasOne(x => x.Assignment)
            .WithMany()
            .IsRequired(false)
            .HasForeignKey(x => x.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.AssignmentId, x.StudentId })
            .IsUnique(false);
    }
}
