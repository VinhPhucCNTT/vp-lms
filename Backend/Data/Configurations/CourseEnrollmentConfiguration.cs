using Backend.Models.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class CourseEnrollmentConfiguration : IEntityTypeConfiguration<CourseEnrollment>
{
    public void Configure(EntityTypeBuilder<CourseEnrollment> builder)
    {
        builder.ToTable("CourseEnrollments");

        builder.HasKey(x => new { x.UserId, x.CourseId });

        builder.Property(x => x.Status)
            .HasConversion<string>();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.Course)
            .WithMany(x => x.Enrollments)
            .IsRequired(false)
            .HasForeignKey(x => x.CourseId);
    }
}
