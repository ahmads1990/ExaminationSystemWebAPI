using ExaminationSystem.API.Models.Requests.Users;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Users;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Old password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters.")
            .MaximumLength(100).WithMessage("New password cannot exceed 100 characters.");
    }
}
