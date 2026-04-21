namespace Backend.Features.Assessments.Dtos.Modify;

public record UpdateQuestionOptionDto(
    string OptionText,
    bool IsCorrect
);
