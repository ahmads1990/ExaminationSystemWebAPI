using ExaminationSystem.API.Models.Requests.Exams;
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
    }
}
