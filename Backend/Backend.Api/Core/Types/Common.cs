namespace Backend.Api.Core.Types;

public interface IEntityResponse
{
    string Id { get; init; }
}

public sealed record GradeResult(
    decimal EarnedPoints,
    bool? IsCorrect);

public sealed record SelectedQuestion(
    long QuestionId,
    int OrderIndex,
    decimal Points);

