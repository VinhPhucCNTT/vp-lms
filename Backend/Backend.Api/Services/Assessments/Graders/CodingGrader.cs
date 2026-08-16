using Backend.Api.Core.Common;
using Backend.Persistence.Entities.Assessments;

namespace Backend.Api.Services.Assessments.Graders;

public sealed class CodingGrader
    : IQuestionGrader
{
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
