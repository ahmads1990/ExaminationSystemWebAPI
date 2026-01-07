using ExaminationSystem.API.Models.Requests.Courses;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Course;

public class AddCourseRequestValidator : AbstractValidator<AddCourseRequest>
{
    public AddCourseRequestValidator()
    {
        RuleFor(c => c.Title).ApplyTitleRules();
        RuleFor(c => c.Description).ApplyDescriptionRules();
        RuleFor(c => c.CreditHours).ApplyCreditHoursRules();
    }
}