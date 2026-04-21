using Backend.Models.Enums;

namespace Backend.Features.Assessments.Dtos.View;

public record ViewQuestionDto(
    Guid Id,
    string QuestionText,
    QuestionType Type,
    decimal Points,
    int OrderIndex, // TODO: Do this at service level
    List<ViewQuestionOptionDto> Options
);
