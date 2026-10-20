namespace Backend.Api.Core.Types;

public interface IEntityResponse
{
    string Id { get; init; }
}

public sealed record GradeResult(
    decimal EarnedPoints,
    bool? IsCorrect);

