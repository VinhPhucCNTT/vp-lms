using Backend.Core.Entities.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("enrollments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.HasIndex(x => new { x.CourseId, x.UserId })
            .HasFilter("IsDeleted = false")
            .IsUnique();

        builder.HasOne(x => x.TAPermissions)
            .WithOne(tp => tp.Enrollment)
            .HasForeignKey<TAPermissions>(tp => tp.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
