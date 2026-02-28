using ExaminationSystem.API.Models.Requests.Questions;
using ExaminationSystem.Domain.Common;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Question;

public class AddQuestionRequestValidator : AbstractValidator<AddQuestionRequest>
{
    public AddQuestionRequestValidator()
    {
        RuleFor(q => q.Body).ApplyBodyRules();
        RuleFor(q => q.Score).ApplyScoreRules();
        RuleFor(q => q.QuestionLevel).ApplyQuestionLevelRules();

        RuleFor(q => q.Choices)
            .NotEmpty()
            .WithMessage("Adding choices is required.")
            .Must(choices => choices.Count() >= Constants.MinChoicesCount)
            .WithMessage($"A question must have at least {Constants.MinChoicesCount} choices.")
            .Must(choices => choices.Count() <= Constants.MaxChoicesCount)
            .WithMessage($"A question cannot have more than {Constants.MaxChoicesCount} choices.")
            .Must(choices => choices.Count(c => c.IsCorrect) == 1)
            .WithMessage("Exactly one choice must be marked as the correct answer.");

        RuleForEach(q => q.Choices).ChildRules(choices =>
        {
            choices.RuleFor(c => c.Body).ApplyChoiceBodyRules();
        });
    }
}
