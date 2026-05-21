using Contracts.Log;
using FluentValidation;

namespace WebApi.Validators;

/// <summary>
/// Validator for CreateLogRequest.
/// </summary>
public class CreateLogRequestValidator : AbstractValidator<CreateLogRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateLogRequestValidator"/> class.
    /// </summary>
    public CreateLogRequestValidator()
    {
        RuleFor(x => x.DateTime)
            .NotEmpty().WithMessage("DateTime is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1))
            .WithMessage("DateTime cannot be more than 1 hour in the future");

        RuleFor(x => x.Margine)
            .NotEmpty().WithMessage("Margin is required");

        RuleFor(x => x.Notes)
            .NotEmpty().WithMessage("Notes are required")
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters");

        RuleFor(x => x.Json)
            .MaximumLength(5000).WithMessage("Json must not exceed 5000 characters");
    }
}