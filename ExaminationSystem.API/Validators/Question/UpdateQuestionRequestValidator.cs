using ExaminationSystem.API.Models.Requests.Questions;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Question;

public class UpdateQuestionRequestValidator : AbstractValidator<UpdateQuestionRequest>
{
    public UpdateQuestionRequestValidator()
    {
        RuleFor(q => q.ID)
            .GreaterThan(0)
            .WithMessage("Question ID is required for updating.");

        // Include all validation from AddQuestionRequestValidator
        Include(new AddQuestionRequestValidator());
    }
}
