using Backend.Api.Core.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Data.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("assignments", t =>
        {
            t.HasCheckConstraint("CK_MinTextLength_NonNegative", "min_text_length >= 0");
            t.HasCheckConstraint("CK_MaxTextLength_NonNegative", "max_text_length >= 0");
            t.HasCheckConstraint("CK_TextLength_MinLessOrEqualMax", "min_text_length <= max_text_length");
            t.HasCheckConstraint("CK_MaxFileCount_GreaterThanZero", "max_file_count > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AllowedExtensions)
            .HasMaxLength(100);

        builder.Property(x => x.SubmissionType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.MinTextLength)
            .HasMaxLength(1000000000);

        builder.Property(x => x.MaxTextLength)
            .HasMaxLength(1000000000);

        // Store grading schema as JSONB
        builder.Property(x => x.GradingSchemaJson)
            .HasColumnType("jsonb")
            .HasColumnName("grading_schema");
    }
}
