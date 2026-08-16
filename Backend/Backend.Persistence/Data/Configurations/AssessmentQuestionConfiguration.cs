using Backend.Persistence.Entities.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Persistence.Data.Configurations;

public class AssessmentQuestionConfiguration : IEntityTypeConfiguration<AssessmentQuestion>
{
    public void Configure(EntityTypeBuilder<AssessmentQuestion> builder)
    {
        builder.ToTable("assessment_questions");

        builder.HasKey(x => x.Id);

        // builder.Property(x => x.QuestionType)
        //     .IsRequired()
        //     .HasMaxLength(30);
        //
        // builder.Property(x => x.QuestionTextMarkdown)
        //     .IsRequired()
        //     .HasColumnType("text");

        builder.Property(x => x.Points)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.HasIndex(x => new { x.AssessmentId, x.OrderIndex })
            .HasFilter("is_deleted = false")
            .IsUnique();
    }
}
