using Contracts.Device;
using FluentValidation;

namespace WebApi.Validators;

/// <summary>
/// Validator for UpdateDeviceRequest.
/// </summary>
public class UpdateDeviceRequestValidator : AbstractValidator<UpdateDeviceRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDeviceRequestValidator"/> class.
    /// </summary>
    public UpdateDeviceRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount must be greater than or equal to 0");
    }
}