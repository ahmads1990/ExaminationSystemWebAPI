using FluentValidation;

namespace ExaminationSystem.API.Validators.Course;

public static class CourseValidationRules
{
    public static IRuleBuilderOptions<T, string> ApplyTitleRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Course title is required.")
            .MaximumLength(100)
            .WithMessage("Course title must not exceed 100 characters.");
    }

    public static IRuleBuilderOptions<T, string> ApplyDescriptionRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Course description is required.")
            .MinimumLength(20)
            .WithMessage("Course description must be at least 20 characters long.")
            .MaximumLength(500)
            .WithMessage("Course description must not exceed 500 characters.");
    }

    public static IRuleBuilderOptions<T, int> ApplyCreditHoursRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(1, 6)
            .WithMessage("Credit hours must be between 1 and 6.");
    }
}