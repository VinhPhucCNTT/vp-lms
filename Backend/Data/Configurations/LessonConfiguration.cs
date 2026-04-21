using Backend.Models.Lessons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("Lessons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContentHtml)
            .IsRequired();

        builder.Property(x => x.VideoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.AttachmentUrl)
            .HasMaxLength(500);

        builder.HasOne(x => x.Activity)
            .WithOne(x => x.Lesson)
            .HasForeignKey<Lesson>(x => x.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
