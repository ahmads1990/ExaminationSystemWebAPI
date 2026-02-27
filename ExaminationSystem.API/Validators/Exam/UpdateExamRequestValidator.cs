using ExaminationSystem.API.Models.Requests.Exams;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Exam;

public class UpdateExamRequestValidator : AbstractValidator<UpdateExamRequest>
{
    public UpdateExamRequestValidator()
    {
        RuleFor(e => e.ExamType).ApplyExamTypeRules();
        RuleFor(e => e.MaxDurationInMinutes).ApplyMaxDurationRules();
        RuleFor(e => e.TotalGrade).ApplyTotalGradeRules();
        RuleFor(e => e.PassingScore).ApplyPassingScoreRules();

        RuleFor(e => e.PassingScore)
            .LessThanOrEqualTo(e => e.TotalGrade)
            .WithMessage("Passing score must be less than or equal to the total grade.");

        RuleFor(e => e.MaxAttempts).ApplyMaxAttemptsRules();
        RuleFor(e => e.DeadlineDate).ApplyDeadlineDateRules();
    }
}
