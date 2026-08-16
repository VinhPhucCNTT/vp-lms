using System.Text.Json;
using Backend.Api.Core.Common;
using Backend.Api.Core.Entities.Assessments;
using Backend.Api.Core.Types;

namespace Backend.Api.Services.Assessments.Graders;

public sealed class DragAndDropGrader
    : IQuestionGrader
{
    public QuestionType Type =>
        QuestionType.DragAndDrop;

    public Result<GradeResult> Grade(
        AssessmentQuestion question,
        AttemptAnswer? answer)
    {
        var content =
            question.QuestionData.Deserialize<DragAndDropQuestion>();

        if (content is null)
            return Result<GradeResult>.Failure(
                QuestionErrors.InvalidContent);

        if (answer is null)
        {
            return Result<GradeResult>.Success(
                new GradeResult(0, false));
        }

        var response =
            answer.AnswerData.Deserialize<DragAndDropAnswer>();

        if (response is null)
        {
            return Result<GradeResult>.Failure(
                new Error(
                    "answer.invalid",
                    "Invalid drag-and-drop answer."));
        }

        var isCorrect = content.Items.All(item =>
            response.Placements.TryGetValue(
                item.Id,
                out var zoneId) &&
            zoneId == item.CorrectZoneId);

        return Result<GradeResult>.Success(
            new GradeResult(
                isCorrect ? question.Points : 0,
                isCorrect));
    }
}

public sealed class DragAndDropAnswer
{
    public Dictionary<string, string> Placements { get; set; } = [];
}
