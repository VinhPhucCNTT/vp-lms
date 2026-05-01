using Backend.Models.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class ResourceCommentConfiguration : IEntityTypeConfiguration<ResourceComment>
{
    public void Configure(EntityTypeBuilder<ResourceComment> builder)
    {
        builder.ToTable("resource_comments");
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContentMarkdown)
            .IsRequired()
            .HasColumnType("text");

        builder.HasOne(x => x.Resource)
            .WithMany(r => r.Comments)
            .HasForeignKey(x => x.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete loops
    }
}
