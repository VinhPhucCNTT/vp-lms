using Backend.Core.Entities.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.ToTable("assessments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InstructionsMarkdown)
            .HasColumnType("text");

        builder.Property(x => x.PassingScore)
            .HasColumnType("decimal(5,2)");

        builder.HasOne(x => x.Resource)
            .WithOne(r => r.Assessment)
            .HasForeignKey<Assessment>(x => x.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Questions)
            .WithOne(q => q.Assessment)
            .HasForeignKey(q => q.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attempts)
            .WithOne(a => a.Assessment)
            .HasForeignKey(a => a.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
