using Backend.Models.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class AttemptAnswerConfiguration 
    : IEntityTypeConfiguration<AttemptAnswer>
{
    public void Configure(EntityTypeBuilder<AttemptAnswer> builder)
    {
        builder.ToTable("AttemptAnswers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AnswerText)
            .HasMaxLength(2000);

        builder.Property(x => x.AwardedPoints)
            .HasPrecision(5, 2);

        builder.Property(x => x.IsCorrect)
            .IsRequired();

        builder.HasOne(x => x.Question)
            .WithMany()
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
