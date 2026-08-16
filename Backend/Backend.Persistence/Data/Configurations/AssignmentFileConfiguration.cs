using Backend.Persistence.Entities.Assignments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Persistence.Data.Configurations;

public class AssignmentFileConfiguration : IEntityTypeConfiguration<AssignmentFile>
{
    public void Configure(EntityTypeBuilder<AssignmentFile> builder)
    {
        builder.ToTable("assignment_files");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.SubmissionId, x.FileId, x.OrderIndex })
            .IsUnique();

        builder.HasOne(x => x.Submission)
            .WithMany(x => x.Files)
            .HasForeignKey(x => x.SubmissionId);
    }
}
