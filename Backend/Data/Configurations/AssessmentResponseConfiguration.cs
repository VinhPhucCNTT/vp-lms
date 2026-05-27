using Backend.Core.Entities.Submissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class AssessmentResponseConfiguration : IEntityTypeConfiguration<AssessmentResponse>
{
    public void Configure(EntityTypeBuilder<AssessmentResponse> builder)
    {
        builder.ToTable("assessment_responses");

        builder.HasKey(x => x.Id);

        // Store as JSONB in PostgreSQL
        builder.Property(x => x.ResponseDataJson)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasColumnName("response_data");

        builder.Property(x => x.Score)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.FeedbackText)
            .HasColumnType("text");

        builder.HasIndex(x => x.ResponseDataJson)
            .HasMethod("GIN");

        builder.HasIndex(x => new { x.AttemptId, x.QuestionId })
            .IsUnique();
    }
}
