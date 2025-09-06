using ExaminationSystem.API.Models.Requests.Auth;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Auth;

public class RegisterInstructorRequestValidator : AbstractValidator<RegisterInstructorRequest>
{
    public RegisterInstructorRequestValidator()
    {
        // Include base user validations
        Include(new RegisterUserBaseValidator());

        // Validate Bio
        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio must not exceed 500 characters.");

        // Validate Specialization
        RuleFor(x => x.Specialization)
            .MaximumLength(200).WithMessage("Specialization must not exceed 200 characters.");
    }
}
