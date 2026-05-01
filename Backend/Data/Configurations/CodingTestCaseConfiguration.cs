using Backend.Models.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class CodingTestCaseConfiguration : IEntityTypeConfiguration<CodingTestCase>
{
    public void Configure(EntityTypeBuilder<CodingTestCase> builder)
    {
        builder.ToTable("coding_test_cases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InputData)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.ExpectedOutput)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.Explanation)
            .HasColumnType("text");

        builder.HasIndex(x => new { x.ChallengeId, x.OrderIndex })
            .IsUnique();

        builder.HasOne(x => x.Challenge)
            .WithMany(c => c.TestCases)
            .HasForeignKey(x => x.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
