using Backend.Models.Enums;

namespace Backend.Features.Assessments.Dtos.Modify;

public record CreateAssessmentDto(
    AssessmentType Type,
    int TimeLimitMinutes,
    int MaxAttempts,
    string? Password,
    decimal PassingScore,
    bool ShuffleQuestions
);
