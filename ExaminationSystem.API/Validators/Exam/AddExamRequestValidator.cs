using ExaminationSystem.API.Models.Requests.Exams;
using FluentValidation;

namespace ExaminationSystem.API.Validators.Exam
{
    public class AddExamRequestValidator : AbstractValidator<AddExamRequest>
    {
        public AddExamRequestValidator()
        {
            RuleFor(e => e.ExamType)
                .IsInEnum()
                .WithMessage("Invalid exam type.");

            RuleFor(e => e.MaxDuration)
                .InclusiveBetween(10, 120)
                .WithMessage("Max duration must be between 10 and 120 minutes.")
                .Must(BeEvenMultipleOfFive)
                .WithMessage("Max duration must be an even number and a multiple of 5.");

            RuleFor(e => e.TotalGrade)
                .InclusiveBetween(10, 150)
                .WithMessage("Total grade must be between 10 and 150.")
                .Must(BeEvenMultipleOfFive)
                .WithMessage("Total grade must be an even number and a multiple of 5.");

            RuleFor(e => e.PassMark)
                .GreaterThan(0)
                .WithMessage("Pass mark must be greater than 0.")
                .Must(BeEvenMultipleOfFive)
                .WithMessage("Pass mark must be an even number and a multiple of 5.");

            RuleFor(e => e.PassMark)
                .LessThanOrEqualTo(e => e.TotalGrade)
                .WithMessage("Pass mark must be less than or equal to the total grade.");

            RuleFor(e => e.DeadlineDate)
                .GreaterThan(DateTime.Now)
                .WithMessage("Deadline date must be in the future.")
                .LessThan(DateTime.Now.AddYears(1))
                .WithMessage("Deadline date must be within one year from today.");

            RuleFor(e => e.CourseID)
                .GreaterThan(0)
                .WithMessage("Course ID must be a positive number.");
        }

        private bool BeEvenMultipleOfFive(int value)
        {
            return value % 2 == 0 && value % 5 == 0;
        }
    }
}
