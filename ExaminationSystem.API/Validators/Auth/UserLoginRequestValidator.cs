using ExaminationSystem.API.Models.Requests.Auth;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Auth;

public class UserLoginRequestValidator : AbstractValidator<UserLoginRequest>
{
    public UserLoginRequestValidator()
    {
        // Validate Email
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        // Validate Password
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
