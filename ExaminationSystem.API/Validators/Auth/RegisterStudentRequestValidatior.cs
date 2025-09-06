using ExaminationSystem.API.Models.Requests.Auth;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Auth;

public class RegisterStudentRequestValidatior : AbstractValidator<RegisterStudentRequest>
{
    public RegisterStudentRequestValidatior()
    {
        // Include base user validations
        Include(new RegisterUserBaseValidator());

        // Validate Level
        RuleFor(x => x.Level)
            .NotEmpty().WithMessage("Level is required.")
            .MaximumLength(50).WithMessage("Level must not exceed 50 characters.");

        // Validate Group
        RuleFor(x => x.Group)
            .MaximumLength(50).WithMessage("Group must not exceed 50 characters.");
    }
}
