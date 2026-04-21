using Backend.Models.Enums;

namespace Backend.Features.Assessments.Dtos.Modify;

public record CreateQuestionDto(
    string QuestionText,
    QuestionType Type,
    decimal Points,
    int OrderIndex
);
