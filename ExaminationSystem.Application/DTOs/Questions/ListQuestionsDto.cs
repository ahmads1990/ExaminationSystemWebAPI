using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.Application.DTOs.Questions;

public class ListQuestionsDto : BasePaginatedDto
{
    public static readonly IReadOnlyList<string> AllowedSortFields =
        [nameof(Question.QuestionLevel), nameof(Question.Score), nameof(Question.ID)];

    public int? ExamID { get; set; }
    public string? Body { get; set; }
}
