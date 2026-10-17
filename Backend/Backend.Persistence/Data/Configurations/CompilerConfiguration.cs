using Backend.Persistence.Entities.Judge;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Persistence.Data.Configurations;

public class CompilerConfiguration : IEntityTypeConfiguration<Compiler>
{
    public void Configure(EntityTypeBuilder<Compiler> builder)
    {
        builder.ToTable("compilers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
