using ExaminationSystem.API.Common;
using ExaminationSystem.API.Models.Requests;
using FluentValidation;

namespace ExaminationSystem.API.Validators;

public class BasePaginatedRequestValidator : AbstractValidator<BasePaginatedRequest>
{
    public BasePaginatedRequestValidator()
    {
        RuleFor(r => r.PageIndex)
            .GreaterThanOrEqualTo(0)
                .WithMessage("PageIndex must be greater than or equal to 0.");

        RuleFor(r => r.PageSize)
            .InclusiveBetween(Constants.MinPageSize, Constants.MaxPageSize)
                .WithMessage($"PageSize must be between {Constants.MinPageSize} and {Constants.MaxPageSize}.");

        RuleFor(r => r.SortDirection)
            .IsInEnum()
                .WithMessage("SortDirection must be 'asc' or 'desc'.");

    }
}
