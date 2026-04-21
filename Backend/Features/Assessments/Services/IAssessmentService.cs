using Backend.Features.Assessments.Dtos.Modify;
using Backend.Features.Assessments.Dtos.Submission;
using Backend.Features.Assessments.Dtos.View;

namespace Backend.Features.Assessments.Services;

public interface IAssessmentService
{
    Task<Guid> CreateAsync(Guid activityId, CreateAssessmentDto dto);

    Task<ViewAssessmentDto?> GetByIdAsync(Guid id);

    Task<List<ViewQuestionDto>?> GetAssessmentQuestionsAsync(Guid id);

    Task<bool> UpdateAsync(Guid id, UpdateAssessmentDto dto);

    Task<bool> DeleteAsync(Guid id);

    Task<bool> RestoreAsync(Guid id);

    Task<bool> HardDeleteAsync(Guid id);

    Task<AssessmentResultDto?> SubmitAttemptAsync(
        Guid assessmentId,
        Guid studentId,
        SubmitAssessmentDto dto);

    Task<Guid?> AddQuestionAsync(
        Guid assessmentId,
        CreateAssessmentQuestionBody dto);

    Task<bool> UpdateQuestionAsync(
        Guid questionId,
        UpdateQuestionDto dto);

    Task<bool> DeleteQuestionAsync(Guid questionId);
}
