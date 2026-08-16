using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Persistence.Data.Configurations;

public class CourseResourceConfiguration : IEntityTypeConfiguration<CourseResource>
{
    public void Configure(EntityTypeBuilder<CourseResource> builder)
    {
        // builder.ToTable(
        //     "course_resources",
        //     t => t.HasCheckConstraint(
        //         "CK_CourseResources_Polymorphic_ExactlyOne",
        //         "(\"lesson_id\" IS NOT NULL)::int + (\"assignment_id\" IS NOT NULL)::int + (\"assessment_id\" IS NOT NULL)::int + (\"problem_id\" IS NOT NULL)::int = 1"
        //     )
        // );
        builder.ToTable("course_resources");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(x => x.Lesson)
            .WithOne(l => l.Resource)
            .HasForeignKey<Lesson>(l => l.ResourceId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(x => x.Assignment)
            .WithOne(a => a.Resource)
            .HasForeignKey<Assignment>(a => a.ResourceId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(x => x.Assessment)
            .WithOne(a => a.Resource)
            .HasForeignKey<Assessment>(a => a.ResourceId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(x => x.Problem)
            .WithOne(c => c.Resource)
            .HasForeignKey<CodingProblem>(c => c.ResourceId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
