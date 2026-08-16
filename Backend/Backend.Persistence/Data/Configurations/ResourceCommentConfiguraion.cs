using Backend.Persistence.Entities.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Persistence.Data.Configurations;

public class ResourceCommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("resource_comments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContentMarkdown)
            .IsRequired()
            .HasColumnType("text");

        builder.HasOne(x => x.Resource)
            .WithMany(r => r.Comments)
            .HasForeignKey(x => x.ActivityId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(x => x.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(true);

        builder.HasOne(x => x.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict) // Prevent cascade delete loops
            .IsRequired(false);
    }
}
