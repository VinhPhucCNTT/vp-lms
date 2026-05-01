using Backend.Models.Submissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class CodingTestResultConfiguration : IEntityTypeConfiguration<CodingTestResult>
{
    public void Configure(EntityTypeBuilder<CodingTestResult> builder)
    {
        builder.ToTable("coding_test_results");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.ActualOutput)
            .HasColumnType("text");

        builder.Property(x => x.ErrorMessage)
            .HasColumnType("text");

        builder.HasOne(x => x.Submission)
            .WithMany(s => s.TestResults)
            .HasForeignKey(x => x.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TestCase)
            .WithMany()
            .HasForeignKey(x => x.TestCaseId)
            .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete test cases
    }
}
