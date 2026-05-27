using Backend.Core.Entities.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class ProblemConfiguration : IEntityTypeConfiguration<Problem>
{
    public void Configure(EntityTypeBuilder<Problem> builder)
    {
        builder.ToTable("problems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProblemStatementMarkdown)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.ConstraintsMarkdown)
            .HasColumnType("text");

        builder.Property(x => x.FunctionSignature)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Language)
            .IsRequired()
            .HasMaxLength(20);
    }
}
