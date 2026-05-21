using Backend.Models.Submissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class CodeExecutionLogConfiguration : IEntityTypeConfiguration<CodeExecutionLog>
{
    public void Configure(EntityTypeBuilder<CodeExecutionLog> builder)
    {
        builder.ToTable("code_execution_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Language)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.CodeSnippet)
            .HasColumnType("text");

        builder.Property(x => x.ErrorMessage)
            .HasColumnType("text");

        builder.Property(x => x.ExecutedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Problem)
            .WithMany()
            .HasForeignKey(x => x.ProblemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
