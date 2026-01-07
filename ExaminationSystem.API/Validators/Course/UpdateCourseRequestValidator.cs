using ExaminationSystem.API.Models.Requests.Courses;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Course;

public class UpdateCourseRequestValidator : AbstractValidator<UpdateCourseRequest>
{
    public UpdateCourseRequestValidator()
    {
        RuleFor(c => c.ID)
            .GreaterThan(0)
            .WithMessage("Course ID must be a positive integer.");

        RuleFor(c => c.Title).ApplyTitleRules();
        RuleFor(c => c.Description).ApplyDescriptionRules();
        RuleFor(c => c.CreditHours).ApplyCreditHoursRules();
    }
}
