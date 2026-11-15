using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Persistence.Data.Configurations;

public class CourseActivityConfiguration : IEntityTypeConfiguration<CourseActivity>
{
    public void Configure(EntityTypeBuilder<CourseActivity> builder)
    {
        // builder.ToTable(
        //     "course_activities",
        //     t => t.HasCheckConstraint(
        //         "CK_CourseActivities_Polymorphic_ExactlyOne",
        //         "(\"lesson_id\" IS NOT NULL)::int + (\"assignment_id\" IS NOT NULL)::int + (\"assessment_id\" IS NOT NULL)::int + (\"problem_id\" IS NOT NULL)::int = 1"
        //     )
        // );
        builder.ToTable("course_activities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(x => x.Lesson)
            .WithOne(l => l.Activity)
            .HasForeignKey<Lesson>(l => l.ActivityId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(x => x.Assignment)
            .WithOne(a => a.Activity)
            .HasForeignKey<Assignment>(a => a.ActivityId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(x => x.Assessment)
            .WithOne(a => a.Activity)
            .HasForeignKey<Assessment>(a => a.ActivityId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(x => x.Problem)
            .WithOne(c => c.Activity)
            .HasForeignKey<CodingProblem>(c => c.ActivityId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
