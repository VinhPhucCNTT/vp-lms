using Backend.Api.Core.Entities.Content;
using Backend.Api.Core.Entities.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Data.Configurations;

public class FileAssetConfiguration : IEntityTypeConfiguration<FileAsset>
{
    public void Configure(EntityTypeBuilder<FileAsset> builder)
    {
        builder.ToTable("file_assets");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
            .WithMany(x => x.FileAssets)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Course)
            .WithOne(x => x.BackgroundFile)
            .HasForeignKey<Course>(x => x.BackgroundFileId)
            .IsRequired(false);

        builder.HasMany(x => x.AssignmentFiles)
            .WithOne(x => x.File)
            .HasForeignKey(x => x.FileId)
            .IsRequired(false);
    }
}
