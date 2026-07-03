using ExaminationSystem.API.Models.Requests.Instructor;
using ExaminationSystem.Application.DTOs.Instructor;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Instructor;

/// <summary>
/// Validator for ListExamSubmissionsRequest.
/// </summary>
public class ListExamSubmissionsRequestValidator : AbstractValidator<ListExamSubmissionsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListExamSubmissionsRequestValidator"/> class.
    /// </summary>
    public ListExamSubmissionsRequestValidator()
    {
        RuleFor(x => x.StudentName)
            .MaximumLength(200);

        RuleFor(x => x.Status)
            .IsInEnum()
                .When(x => x.Status.HasValue)
            .WithMessage("Status must be a valid ExamAttemptStatus.");

        RuleFor(x => x.OrderBy)
            .Must(v => ListExamSubmissionsDto.AllowedSortFields.Contains(v))
            .When(x => !string.IsNullOrEmpty(x.OrderBy))
            .WithMessage($"OrderBy must be one of: {string.Join(", ", ListExamSubmissionsDto.AllowedSortFields)}.");
    }
}
