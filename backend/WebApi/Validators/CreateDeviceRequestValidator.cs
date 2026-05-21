using Contracts.Device;
using FluentValidation;

namespace WebApi.Validators;

/// <summary>
/// Validator for CreateDeviceRequest.
/// </summary>
public class CreateDeviceRequestValidator : AbstractValidator<CreateDeviceRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDeviceRequestValidator"/> class.
    /// </summary>
    public CreateDeviceRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount must be greater than or equal to 0");
    }
}