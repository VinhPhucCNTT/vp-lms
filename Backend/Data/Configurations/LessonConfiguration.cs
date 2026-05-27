using Backend.Core.Entities.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("lessons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContentMarkdown)
            .IsRequired()
            .HasColumnType("text");

        builder.HasOne(x => x.Resource)
            .WithOne(r => r.Lesson)
            .HasForeignKey<Lesson>(x => x.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
