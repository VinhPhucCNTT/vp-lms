namespace Backend.Features.Assessments.Dtos.Modify;

public record CreateQuestionOptionDto(
    string OptionText,
    bool IsCorrect
);
