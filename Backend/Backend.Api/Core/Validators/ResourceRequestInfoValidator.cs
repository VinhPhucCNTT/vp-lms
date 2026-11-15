using Backend.Api.Core.Types;
using FluentValidation;

namespace Backend.Api.Core.Validators;

public class ActivityRequestInfoValidator : AbstractValidator<ActivityRequestInfo>
{
    public ActivityRequestInfoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Activity must be assigned a name.");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Invalid index value.");
    }
}
