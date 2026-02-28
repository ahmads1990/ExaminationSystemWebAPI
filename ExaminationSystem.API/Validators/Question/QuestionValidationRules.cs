using ExaminationSystem.Domain.Common;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Question;

public static class QuestionValidationRules
{
    public static IRuleBuilderOptions<T, string> ApplyBodyRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Question body is required.")
            .Length(Constants.MinQuestionBodyLength, Constants.MaxQuestionBodyLength)
            .WithMessage($"Question body must be between {Constants.MinQuestionBodyLength} and {Constants.MaxQuestionBodyLength} characters.");
    }

    public static IRuleBuilderOptions<T, int> ApplyScoreRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(Constants.MinQuestionScore, Constants.MaxQuestionScore)
            .WithMessage($"Score must be between {Constants.MinQuestionScore} and {Constants.MaxQuestionScore}.");
    }

    public static IRuleBuilderOptions<T, QuestionLevel> ApplyQuestionLevelRules<T>(this IRuleBuilder<T, QuestionLevel> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum()
            .WithMessage("Question level must be one of: Easy (0), Medium (1), Hard (2).");
    }

    public static IRuleBuilderOptions<T, string> ApplyChoiceBodyRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Choice body is required.")
            .MaximumLength(Constants.MaxChoiceBodyLength)
            .WithMessage($"Choice body must not exceed {Constants.MaxChoiceBodyLength} characters.");
    }
}
