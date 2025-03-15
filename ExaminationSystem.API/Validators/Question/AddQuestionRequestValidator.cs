using ExaminationSystem.API.Models.Requests.Questions;
using ExaminationSystem.Domain.Common;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Question;

public class AddQuestionRequestValidator : AbstractValidator<AddQuestionRequest>
{
    public AddQuestionRequestValidator()
    {
        RuleFor(q => q.Body)
            .NotEmpty().WithMessage("Question body is required.")
            .Length(5, 100).WithMessage("Question body must be between 5 and 100 characters.");

        RuleFor(q => q.Score)
            .InclusiveBetween(1, 10)
            .WithMessage("Score must be between 1 and 10.");

        RuleFor(q => q.QuestionLevel)
            .IsInEnum()
            .WithMessage("Question level is invalid.");

        RuleFor(q => q.Choices)
            .NotEmpty()
            .WithMessage("Adding choices is required.")
            .Must(choices => choices.Count() >= Constants.MinChoicesCount)
            .WithMessage($"A question must have at least {Constants.MinChoicesCount} choices.")
            .Must(choices => choices.Count() <= Constants.MaxChoicesCount)
            .WithMessage($"A question cannot have more than {Constants.MaxChoicesCount} choices.");

        RuleFor(q => q.AnswerOrder)
            .Must((q, order) => order >= 0 && order < q.Choices.Count())
            .WithMessage(q => $"AnswerOrder must be between 0 and {Constants.MaxChoicesCount}.");

        // Inline validator for choices
        RuleForEach(q => q.Choices).ChildRules(choices =>
        {
            choices.RuleFor(c => c.Body)
                .NotEmpty().WithMessage("Choice body is required.")
                .MaximumLength(200).WithMessage("Choice body must not exceed 200 characters.");
        });
    }
}
