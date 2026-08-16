using Backend.Api.Core.Types;
using FluentValidation;

namespace Backend.Api.Core.Validators;

public class AssignmentSubmissionValidator : AbstractValidator<SubmissionRequest>
{
    public AssignmentSubmissionValidator()
    {
    }
}
