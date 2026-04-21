namespace Backend.Features.Assessments.Dtos.View;

public record ViewQuestionOptionDto(
    Guid Id,
    string OptionText,
    bool IsCorrect
);
