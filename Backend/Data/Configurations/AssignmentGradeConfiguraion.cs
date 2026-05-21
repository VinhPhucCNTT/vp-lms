using Backend.Models.Submissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class AssignmentGradeConfiguration : IEntityTypeConfiguration<AssignmentGrade>
{
    public void Configure(EntityTypeBuilder<AssignmentGrade> builder)
    {
        builder.ToTable("assignment_grades");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Score)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.FeedbackText)
            .HasColumnType("text");

        builder.HasOne(x => x.Submission)
            .WithOne(s => s.Grade)
            .HasForeignKey<AssignmentGrade>(x => x.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(x => x.Grader)
            .WithMany()
            .HasForeignKey(x => x.GraderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
