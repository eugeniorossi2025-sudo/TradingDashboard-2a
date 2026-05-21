using Contracts.Configuration;
using FluentValidation;

namespace WebApi.Validators;

/// <summary>
/// Validator for UpdateConfigurationRequest.
/// </summary>
public class UpdateConfigurationRequestValidator : AbstractValidator<UpdateConfigurationRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateConfigurationRequestValidator"/> class.
    /// </summary>
    public UpdateConfigurationRequestValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Value)
            .MaximumLength(1000).WithMessage("Value must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Value));

        RuleFor(x => x.Pos)
            .GreaterThanOrEqualTo(0).WithMessage("Position must be greater than or equal to 0")
            .When(x => x.Pos.HasValue);
    }
}