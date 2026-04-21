using Backend.Models.Enums;

namespace Backend.Features.Assessments.Dtos.View;

public record ViewAssessmentDto(
    Guid Id,
    AssessmentType Type,
    int TimeLimitMinutes,
    int MaxAttempts,
    decimal PassingScore,
    bool ShuffleQuestions
);
