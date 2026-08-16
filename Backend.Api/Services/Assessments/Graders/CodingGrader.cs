using Backend.Api.Core.Common;
using Backend.Api.Core.Entities.Assessments;

namespace Backend.Api.Services.Assessments.Graders;

public sealed class CodingGrader
    : IQuestionGrader
{
    private readonly IJudgeService _judge;

    public CodingGrader(IJudgeService judge)
    {
        _judge = judge;
    }

    public QuestionType Type =>
        QuestionType.Coding;

    public Result<GradeResult> Grade(
        AssessmentQuestion question,
        AttemptAnswer? answer)
    {
        // Coding assessments are asynchronous in practice.
        // The attempt should be submitted to the Judge system,
        // rather than trying to synchronously execute code here.

        return Result<GradeResult>.Success(
            new GradeResult(0, null));
    }
}
