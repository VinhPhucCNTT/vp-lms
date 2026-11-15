using Backend.Persistence.Entities.Learning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Persistence.Data.Configurations;

public class ActivityProgressConfiguration : IEntityTypeConfiguration<ActivityProgress>
{
    public void Configure(EntityTypeBuilder<ActivityProgress> builder)
    {
        builder.ToTable("activity_progress");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserId, x.ActivityId }).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Activity)
            .WithMany(r => r.Progress)
            .HasForeignKey(x => x.ActivityId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
