using Backend.Persistence.Entities.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Persistence.Data.Configurations;

public class AttemptAnswerConfiguration : IEntityTypeConfiguration<AttemptAnswer>
{
    public void Configure(EntityTypeBuilder<AttemptAnswer> builder)
    {
        builder.ToTable("attempt_answers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EarnedPoints)
            .HasColumnType("decimal(5,2)");

        builder.HasIndex(x => new { x.GraderId, x.AttemptQuestionId })
            .IsUnique();
    }
}
