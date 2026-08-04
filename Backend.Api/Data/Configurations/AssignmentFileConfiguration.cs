using Backend.Api.Core.Entities.Assignments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Data.Configurations;

public class AssignmentFileConfiguration : IEntityTypeConfiguration<AssignmentFile>
{
    public void Configure(EntityTypeBuilder<AssignmentFile> builder)
    {
        builder.ToTable("assignment_files");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.AssignmentId, x.FileId, x.OrderIndex })
            .IsUnique();
    }
}
