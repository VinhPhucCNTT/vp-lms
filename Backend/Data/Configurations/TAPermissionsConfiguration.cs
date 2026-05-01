using Backend.Models.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class TAPermissionsConfiguration : IEntityTypeConfiguration<TAPermissions>
{
    public void Configure(EntityTypeBuilder<TAPermissions> builder)
    {
        builder.ToTable("ta_permissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.GrantedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(x => x.Enrollment)
            .WithOne(e => e.TAPermissions)
            .HasForeignKey<TAPermissions>(x => x.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.GrantedByUser)
            .WithMany()
            .HasForeignKey(x => x.GrantedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
