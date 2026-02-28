using ExaminationSystem.API.Models.Requests.Questions;
using ExaminationSystem.Domain.Common;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Question;

public class ListQuestionsRequestValidator : AbstractValidator<ListQuestionsRequest>
{
    public ListQuestionsRequestValidator()
    {
        RuleFor(x => x.Body)
            .MaximumLength(Constants.MaxQuestionBodyLength);

        RuleFor(x => x.ExamID)
            .GreaterThan(0)
                .When(x => x.ExamID.HasValue);
    }
}
