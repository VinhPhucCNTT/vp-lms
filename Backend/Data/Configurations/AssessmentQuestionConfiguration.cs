using Backend.Models.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class AssessmentQuestionConfiguration 
    : IEntityTypeConfiguration<AssessmentQuestion>
{
    public void Configure(EntityTypeBuilder<AssessmentQuestion> builder)
    {
        builder.ToTable("AssessmentQuestions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuestionText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Type)
            .HasConversion<string>();

        builder.Property(x => x.Points)
            .HasPrecision(5, 2);

        builder.HasIndex(x => new { x.AssessmentId, x.OrderIndex })
            .IsUnique();

        builder.HasMany(x => x.Options)
            .WithOne(x => x.Question)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
