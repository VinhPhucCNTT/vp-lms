using Backend.Persistence.Entities.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Persistence.Data.Configurations;

public class RecentCourseConfiguration : IEntityTypeConfiguration<RecentCourse>
{
    public void Configure(EntityTypeBuilder<RecentCourse> builder)
    {
        builder.ToTable("recent_courses");

        builder.HasKey(x => new { x.UserId, x.CourseId });

        builder.Property(x => x.LastAccessedAt)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Course)
            .WithMany()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.LastAccessedAt
        });
    }
}
