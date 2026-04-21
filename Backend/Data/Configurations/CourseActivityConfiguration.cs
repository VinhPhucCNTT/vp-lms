using Backend.Models.Assessments;
using Backend.Models.Assignments;
using Backend.Models.Courses;
using Backend.Models.Lessons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class CourseActivityConfiguration : IEntityTypeConfiguration<CourseActivity>
{
    public void Configure(EntityTypeBuilder<CourseActivity> builder)
    {
        builder.ToTable("CourseActivities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Type)
            .HasConversion<string>();

        builder.HasIndex(x => new { x.ModuleId, x.OrderIndex })
            .IsUnique();

        builder.HasOne(x => x.Lesson)
            .WithOne(x => x.Activity)
            .HasForeignKey<Lesson>(x => x.ActivityId);

        builder.HasOne(x => x.Assessment)
            .WithOne(x => x.Activity)
            .HasForeignKey<Assessment>(x => x.ActivityId);

        builder.HasOne(x => x.Assignment)
            .WithOne(x => x.Activity)
            .HasForeignKey<Assignment>(x => x.ActivityId);
    }
}
