using Backend.Api.Core.Types;
using FluentValidation;

namespace Backend.Api.Core.Validators;

public class CodingProblemRequestValidator : AbstractValidator<CodingProblemRequest>
{
    public CodingProblemRequestValidator()
    {
        RuleFor(x => x.ResourceInfo)
            .SetValidator(new ResourceRequestInfoValidator);

        RuleFor(x => x.Info.ProblemStatementMarkdown)
            .NotEmpty().WithMessage("Problem statement is required.");
    }
}
