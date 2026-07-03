using ExaminationSystem.API.Models.Requests.Instructor;
using ExaminationSystem.Application.DTOs.Instructor;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Instructor;

/// <summary>
/// Validator for ListInstructorCoursesRequest.
/// </summary>
public class ListInstructorCoursesRequestValidator : AbstractValidator<ListInstructorCoursesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListInstructorCoursesRequestValidator"/> class.
    /// </summary>
    public ListInstructorCoursesRequestValidator()
    {
        RuleFor(x => x.CourseName)
            .MaximumLength(200);

        RuleFor(x => x.OrderBy)
            .Must(v => ListInstructorCoursesDto.AllowedSortFields.Contains(v))
            .When(x => !string.IsNullOrEmpty(x.OrderBy))
            .WithMessage($"OrderBy must be one of: {string.Join(", ", ListInstructorCoursesDto.AllowedSortFields)}.");
    }
}
