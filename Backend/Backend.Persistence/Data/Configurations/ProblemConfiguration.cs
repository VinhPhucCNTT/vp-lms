using System.Text.Json;
using Backend.Persistence.Common;
using Backend.Persistence.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Persistence.Data.Configurations;

public class ProblemConfiguration : IEntityTypeConfiguration<CodingProblem>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly ValueComparer<LanguageProfile[]> ProfilesComparer =
        new(
            (a, b) => JsonSerializer.Serialize(a, JsonOptions)
                   == JsonSerializer.Serialize(b, JsonOptions),

            v => v == null
                ? 0
                : JsonSerializer.Serialize(v, JsonOptions).GetHashCode(),

            v => v == null
                ? Array.Empty<LanguageProfile>()
                : JsonSerializer.Deserialize<LanguageProfile[]>(
                    JsonSerializer.Serialize(v, JsonOptions),
                    JsonOptions) ?? Array.Empty<LanguageProfile>()
        );

    public void Configure(EntityTypeBuilder<CodingProblem> builder)
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

        builder.Property(x => x.LanguageProfile)
            .HasConversion(
                profiles => JsonSerializer.Serialize(
                    profiles, JsonOptions),

                json => JsonSerializer.Deserialize<LanguageProfile[]>(
                    json, JsonOptions) ?? Array.Empty<LanguageProfile>()
            )
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(ProfilesComparer);
    }
}
