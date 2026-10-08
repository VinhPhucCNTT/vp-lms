using Backend.Api.Core.Types;
using FluentValidation;

namespace Backend.Api.Core.Validators;

public class CodingProblemInfoValidator : AbstractValidator<CodingProblemInfo>
{
    public CodingProblemInfoValidator()
    {
        RuleFor(x => x.ProblemStatementMarkdown)
            .NotEmpty().WithMessage("Problem statement is required.");
        
        RuleFor(x => x.TimeLimitMs)
            .GreaterThan(0).WithMessage("Time limit must be greater than zero.");

        RuleFor(x => x.MemoryLimitMs)
            .GreaterThan(0).WithMessage("Memory limit must be greater than zero.");
    }
}
