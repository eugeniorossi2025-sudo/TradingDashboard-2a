using Contracts.User;
using FluentValidation;

namespace WebApi.Validators;

/// <summary>
/// Validator for UpdateUserRequest.
/// </summary>
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRequestValidator"/> class.
    /// </summary>
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.LastLogin)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("LastLogin cannot be in the future")
            .When(x => x.LastLogin.HasValue);
    }
}