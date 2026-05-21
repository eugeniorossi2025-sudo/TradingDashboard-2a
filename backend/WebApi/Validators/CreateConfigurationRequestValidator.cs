using Contracts.Configuration;
using FluentValidation;

namespace WebApi.Validators;

/// <summary>
/// Validator for CreateConfigurationRequest.
/// </summary>
public class CreateConfigurationRequestValidator : AbstractValidator<CreateConfigurationRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateConfigurationRequestValidator"/> class.
    /// </summary>
    public CreateConfigurationRequestValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required")
            .MaximumLength(100).WithMessage("Key must not exceed 100 characters")
            .Matches("^[a-zA-Z0-9_.-]+$")
            .WithMessage("Key can only contain letters, numbers, dots, dashes, and underscores");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required")
            .MaximumLength(1000).WithMessage("Value must not exceed 1000 characters");

        RuleFor(x => x.Pos)
            .GreaterThanOrEqualTo(0).WithMessage("Position must be greater than or equal to 0");
    }
}