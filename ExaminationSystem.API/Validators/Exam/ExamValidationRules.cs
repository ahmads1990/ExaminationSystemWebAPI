using ExaminationSystem.Domain.Common;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Exam;

public static class ExamValidationRules
{
    public static IRuleBuilderOptions<T, ExamType> ApplyExamTypeRules<T>(this IRuleBuilder<T, ExamType> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum()
            .WithMessage("Invalid exam type.");
    }

    public static IRuleBuilderOptions<T, int> ApplyMaxDurationRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(Constants.MinDurationInMinutes, Constants.MaxDurationInMinutes)
            .WithMessage($"Max duration must be between {Constants.MinDurationInMinutes} and {Constants.MaxDurationInMinutes} minutes.")
            .Must(BeEvenMultipleOfFive)
            .WithMessage("Max duration must be an even number and a multiple of 5.");
    }

    public static IRuleBuilderOptions<T, int> ApplyTotalGradeRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(Constants.MinTotalGrade, Constants.MaxTotalGrade)
            .WithMessage($"Total grade must be between {Constants.MinTotalGrade} and {Constants.MaxTotalGrade}.");
    }

    public static IRuleBuilderOptions<T, decimal> ApplyPassingScoreRules<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage("Passing score must be greater than 0.");
    }

    public static IRuleBuilderOptions<T, int> ApplyMaxAttemptsRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(Constants.MinAttempts, Constants.MaxAttempts)
            .WithMessage($"Max attempts must be between {Constants.MinAttempts} and {Constants.MaxAttempts}.");
    }

    public static IRuleBuilderOptions<T, DateTime> ApplyDeadlineDateRules<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(DateTime.Now)
            .WithMessage("Deadline date must be in the future.")
            .LessThan(DateTime.Now.AddYears(Constants.MaxDeadlineYears))
            .WithMessage($"Deadline date must be within {Constants.MaxDeadlineYears} year(s) from today.");
    }

    private static bool BeEvenMultipleOfFive(int value)
    {
        return value % 2 == 0 && value % 5 == 0;
    }
}
