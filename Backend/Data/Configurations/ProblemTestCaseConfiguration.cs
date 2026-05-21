using Backend.Models.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class ProblemTestCaseConfiguration : IEntityTypeConfiguration<ProblemTestCase>
{
    public void Configure(EntityTypeBuilder<ProblemTestCase> builder)
    {
        builder.ToTable("problem_test_cases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InputData)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.ExpectedOutput)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.Explanation)
            .HasColumnType("text");

        builder.HasIndex(x => new { x.ProblemId, x.OrderIndex })
            .IsUnique();

        builder.HasOne(x => x.Problem)
            .WithMany(c => c.TestCases)
            .HasForeignKey(x => x.ProblemId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
