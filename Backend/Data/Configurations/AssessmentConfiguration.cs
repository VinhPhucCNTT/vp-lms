using Backend.Models.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.ToTable("Assessments");
        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasConversion<string>();

        builder.Property(x => x.TimeLimitMinutes)
            .IsRequired();

        builder.Property(x => x.MaxAttempts)
            .IsRequired();

        builder.Property(x => x.Password)
            .HasMaxLength(100);

        builder.Property(x => x.PassingScore)
            .HasPrecision(5, 2);

        builder.HasOne(x => x.Activity)
            .WithOne(x => x.Assessment)
            .IsRequired(false)
            .HasForeignKey<Assessment>(x => x.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Questions)
            .WithOne(x => x.Assessment)
            .IsRequired(false)
            .HasForeignKey(x => x.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
