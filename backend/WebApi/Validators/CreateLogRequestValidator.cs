using Contracts.Log;
using FluentValidation;

namespace WebApi.Validators;

public class CreateLogRequestValidator : AbstractValidator<CreateLogRequest>
{
    public CreateLogRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(4000);

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(100);
    }
}
