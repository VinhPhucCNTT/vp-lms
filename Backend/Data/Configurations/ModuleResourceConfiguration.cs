using Backend.Models.Courses;
using Backend.Models.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class ModuleResourceConfiguration : IEntityTypeConfiguration<ModuleResource>
{
    public void Configure(EntityTypeBuilder<ModuleResource> builder)
    {
        builder.ToTable("module_resources");
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ResourceType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasColumnType("text");

        builder.Property(x => x.AccessPasswordHash)
            .HasMaxLength(255);

        builder.HasIndex(x => new { x.ModuleId, x.OrderIndex })
            .IsUnique();

        builder.HasOne(x => x.Lesson)
            .WithOne(l => l.Resource)
            .HasForeignKey<Lesson>(l => l.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Assignment)
            .WithOne(a => a.Resource)
            .HasForeignKey<Assignment>(a => a.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Coding)
            .WithOne(c => c.Resource)
            .HasForeignKey<Coding>(c => c.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Assessment)
            .WithOne(a => a.Resource)
            .HasForeignKey<Assessment>(a => a.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
