using ExaminationSystem.API.Models.Requests.StudentCourses;
using FluentValidation;

namespace ExaminationSystem.API.Validators.StudentCourses;

public class StudentEnrollInCourseRequestValidator : AbstractValidator<StudentEnrollInCourseRequest>
{
    public StudentEnrollInCourseRequestValidator()
    {
        RuleFor(x => x.CourseID)
            .NotNull()
                .WithMessage("CourseID is required.")
            .GreaterThan(0)
                .WithMessage("CourseID must be greater than 0.");
    }
}
