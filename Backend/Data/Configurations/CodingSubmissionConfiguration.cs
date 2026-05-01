using Backend.Models.Submissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class CodingSubmissionConfiguration : IEntityTypeConfiguration<CodingSubmission>
{
    public void Configure(EntityTypeBuilder<CodingSubmission> builder)
    {
        builder.ToTable("coding_submissions");

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

        builder.HasOne(x => x.Challenge)
            .WithMany(c => c.Submissions)
            .HasForeignKey(x => x.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);

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
