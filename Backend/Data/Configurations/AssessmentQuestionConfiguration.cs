using Backend.Models.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class AssessmentQuestionConfiguration : IEntityTypeConfiguration<AssessmentQuestion>
{
    public void Configure(EntityTypeBuilder<AssessmentQuestion> builder)
    {
        builder.ToTable("assessment_questions");
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuestionType)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.QuestionTextMarkdown)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.Points)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        // Store as JSONB in PostgreSQL
        builder.Property(x => x.QuestionDataJson)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasColumnName("question_data");

        builder.HasIndex(x => new { x.AssessmentId, x.OrderIndex })
            .IsUnique();

        builder.HasIndex(x => x.QuestionDataJson)
            .HasMethod("GIN");
    }
}
