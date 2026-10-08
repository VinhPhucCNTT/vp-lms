using Backend.Api.Core.Types;
using FluentValidation;

namespace Backend.Api.Core.Validators;

public class ResourceRequestInfoValidator : AbstractValidator<ResourceRequestInfo>
{
    public ResourceRequestInfoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Resource must be assigned a name.");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Invalid index value.");
    }
}
