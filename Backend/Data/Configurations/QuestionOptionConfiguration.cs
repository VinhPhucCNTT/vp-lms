using Backend.Models.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class QuestionOptionConfiguration 
    : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("QuestionOptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OptionText)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.IsCorrect)
            .IsRequired();
    }
}
