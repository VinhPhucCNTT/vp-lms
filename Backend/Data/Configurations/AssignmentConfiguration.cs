using Backend.Models.Assignments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class AssignmentConfiguration 
    : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("Assignments");
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Instructions)
            .IsRequired();

        builder.Property(x => x.MaxPoints)
            .HasPrecision(5, 2);

        builder.Property(x => x.AllowLateSubmission)
            .IsRequired();

        builder.HasOne(x => x.Activity)
            .WithOne(x => x.Assignment)
            .IsRequired(false)
            .HasForeignKey<Assignment>(x => x.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
