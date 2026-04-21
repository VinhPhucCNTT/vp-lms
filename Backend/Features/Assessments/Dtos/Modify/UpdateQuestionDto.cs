using Backend.Models.Enums;

namespace Backend.Features.Assessments.Dtos.Modify;

public record UpdateQuestionDto(
    string QuestionText,
    QuestionType Type,
    decimal Points,
    int OrderIndex,
    List<UpdateQuestionOptionDto> Options
);
