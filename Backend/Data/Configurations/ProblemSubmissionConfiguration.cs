using Backend.Core.Entities.Submissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class ProblemSubmissionConfiguration : IEntityTypeConfiguration<ProblemSubmission>
{
    public void Configure(EntityTypeBuilder<ProblemSubmission> builder)
    {
        builder.ToTable("problem_submissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SubmittedCode)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.Language)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.HasIndex(x => new { x.ProblemId, x.UserId });

        builder.HasOne(x => x.Problem)
            .WithMany(c => c.Submissions)
            .HasForeignKey(x => x.ProblemId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.TestResults)
            .WithOne(tr => tr.Submission)
            .HasForeignKey(tr => tr.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
