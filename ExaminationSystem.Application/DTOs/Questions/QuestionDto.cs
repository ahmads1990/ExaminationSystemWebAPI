using ExaminationSystem.Application.DTOs.Choices;

namespace ExaminationSystem.Application.DTOs.Questions;

/// <summary>
/// Data transfer object for question details.
/// </summary>
public class QuestionDto
{
    /// <summary>
    /// The unique identifier of the question.
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// The textual content of the question.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// The score value assigned to this question.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// The difficulty level of the question (e.g., Easy, Medium, Hard).
    /// </summary>
    public QuestionLevel QuestionLevel { get; set; }

    /// <summary>
    /// The list of multiple-choice answers for this question.
    /// </summary>
    public IEnumerable<ChoiceDto> Choices { get; set; } = new List<ChoiceDto>();
}
