using Backend.Core.Entities.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class ModuleConfiguration : IEntityTypeConfiguration<CourseModule>
{
    public void Configure(EntityTypeBuilder<CourseModule> builder)
    {
        builder.ToTable("modules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasColumnType("text");

        builder.HasIndex(x => new { x.CourseId, x.OrderIndex })
            .IsUnique();

        builder.HasMany(x => x.Resources)
            .WithOne(r => r.Module)
            .HasForeignKey(r => r.ModuleId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
