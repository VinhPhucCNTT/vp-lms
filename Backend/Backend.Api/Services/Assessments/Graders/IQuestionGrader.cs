using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Assessments;

namespace Backend.Api.Services.Assessments.Graders;

public interface IQuestionGrader
{
    QuestionType Type { get; }

    Result<GradeResult> Grade(
        AssessmentQuestion question,
        AttemptAnswer? answer);
}

