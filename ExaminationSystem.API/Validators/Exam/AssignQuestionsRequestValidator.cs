using ExaminationSystem.API.Models.Requests.Exams;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Exam;

public class AssignQuestionsRequestValidator : AbstractValidator<AssignQuestionsRequest>
{
    public AssignQuestionsRequestValidator()
    {
        RuleFor(x => x.ExamId)
            .GreaterThan(0)
            .WithMessage("Exam ID is required.");

        RuleFor(x => x.QuestionIds)
            .NotEmpty()
            .WithMessage("At least one question ID is required.");

        RuleForEach(x => x.QuestionIds)
            .GreaterThan(0)
            .WithMessage("Each question ID must be greater than 0.");
    }
}
