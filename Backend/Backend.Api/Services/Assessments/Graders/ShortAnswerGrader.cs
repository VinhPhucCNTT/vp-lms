using System.Text.Json;
using Backend.Api.Core.Common;
using Backend.Persistence.Entities.Assessments;
using Backend.Api.Core.Types;

namespace Backend.Api.Services.Assessments.Graders;

public sealed class ShortAnswerGrader
    : IQuestionGrader
{
    public QuestionType Type =>
        QuestionType.ShortAnswer;

    public Result<GradeResult> Grade(
        AssessmentQuestion question,
        AttemptAnswer? answer)
    {
        var content =
            question.QuestionData.Deserialize<ShortAnswerQuestion>();

        if (content is null)
            return Result<GradeResult>.Failure(
                QuestionErrors.InvalidContent);

        if (answer is null)
        {
            return Result<GradeResult>.Success(
                new GradeResult(0, false));
        }

        var response =
            answer.AnswerData.Deserialize<ShortAnswerResponse>();

        if (response is null)
        {
            return Result<GradeResult>.Failure(
                new Error(
                    "answer.invalid",
                    "Invalid short-answer response."));
        }

        var submitted = Normalize(
            response.Text ?? string.Empty,
            content.IsCaseSensitive);

        var correct = content.AcceptedAnswers
            .Select(x =>
                Normalize(x, content.IsCaseSensitive))
            .Any(x => x == submitted);

        return Result<GradeResult>.Success(
            new GradeResult(
                correct ? question.Points : 0,
                correct));
    }

    private static string Normalize(
        string value,
        bool caseSensitive)
    {
        value = value.Trim();

        return caseSensitive
            ? value
            : value.ToLowerInvariant();
    }
}

public sealed class ShortAnswerResponse
{
    public string? Text { get; set; }
}
