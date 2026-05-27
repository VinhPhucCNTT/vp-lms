using Backend.Core.Entities.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("assignments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InstructionsMarkdown)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.MaxScore)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.AllowedFileTypes)
            .HasMaxLength(255);

        builder.Property(x => x.SubmissionType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        // Store grading schema as JSONB
        builder.Property(x => x.GradingSchemaJson)
            .HasColumnType("jsonb")
            .HasColumnName("grading_schema");
    }
}
