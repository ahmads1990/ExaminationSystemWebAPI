using ExaminationSystem.API.Models.Requests.Exams;
using ExaminationSystem.Domain.Common;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Exam;

/// <summary>
/// Validates a <see cref="PublishExamRequest"/> ensuring valid exam ID and optional publish date.
/// </summary>
public class PublishExamRequestValidator : AbstractValidator<PublishExamRequest>
{
    public PublishExamRequestValidator()
    {
        RuleFor(p => p.ID)
            .GreaterThan(0)
            .WithMessage("Exam ID must be a positive number.");

        RuleFor(p => p.PublishDate)
            .GreaterThan(DateTime.Now)
            .WithMessage("Publish date must be in the future.")
            .LessThan(DateTime.Now.AddYears(Constants.MaxDeadlineYears))
            .WithMessage($"Publish date must be within {Constants.MaxDeadlineYears} year(s) from today.")
            .When(p => p.PublishDate.HasValue);
    }
}
