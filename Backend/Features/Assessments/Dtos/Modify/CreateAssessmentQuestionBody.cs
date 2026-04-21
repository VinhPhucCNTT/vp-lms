using Backend.Models.Enums;

namespace Backend.Features.Assessments.Dtos.Modify;

public record CreateAssessmentQuestionBody(
    string QuestionText,
    QuestionType Type,
    decimal Points,
    int OrderIndex,
    List<CreateQuestionOptionDto> Options
);
