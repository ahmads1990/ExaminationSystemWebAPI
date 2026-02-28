using ExaminationSystem.API.Models.Requests.Courses;
using ExaminationSystem.Application.DTOs.Courses;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Course;

public class ListCoursesRequestValidator : AbstractValidator<ListCoursesRequest>
{
    public ListCoursesRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.CreditHours)
            .GreaterThan(0)
                .When(x => x.CreditHours.HasValue)
            .LessThanOrEqualTo(6)
                .When(x => x.CreditHours.HasValue);

        RuleFor(x => x.InstructorID)
            .GreaterThan(0)
                .When(x => x.InstructorID.HasValue);

        RuleFor(x => x.OrderBy)
            .Must(v => ListCoursesDto.AllowedSortFields.Contains(v))
            .When(x => !string.IsNullOrEmpty(x.OrderBy))
            .WithMessage($"OrderBy must be one of: {string.Join(", ", ListCoursesDto.AllowedSortFields)}.");
    }
}
