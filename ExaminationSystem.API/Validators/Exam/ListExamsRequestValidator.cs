using ExaminationSystem.API.Models.Requests.Exams;
using ExaminationSystem.Application.DTOs.Exams;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Exam;

public class ListExamsRequestValidator : AbstractValidator<ListExamsRequest>
{
    public ListExamsRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200);

        RuleFor(x => x.ExamType)
            .IsInEnum()
                .When(x => x.ExamType.HasValue);

        RuleFor(x => x.OrderBy)
            .Must(v => ListExamsDto.AllowedSortFields.Contains(v))
            .When(x => !string.IsNullOrEmpty(x.OrderBy))
            .WithMessage($"OrderBy must be one of: {string.Join(", ", ListExamsDto.AllowedSortFields)}.");
    }
}
