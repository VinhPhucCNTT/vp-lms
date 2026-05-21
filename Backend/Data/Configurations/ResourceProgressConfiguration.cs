using Backend.Models.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class ResourceProgressConfiguration : IEntityTypeConfiguration<ResourceProgress>
{
    public void Configure(EntityTypeBuilder<ResourceProgress> builder)
    {
        builder.ToTable("resource_progress"); 

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserId, x.ResourceId, x.IsCompleted });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Resource)
            .WithMany(r => r.Progress)
            .HasForeignKey(x => x.ResourceId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
