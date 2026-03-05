namespace ExaminationSystem.Application.DTOs.StudentExams;

/// <summary>
/// Represents a question as seen by the student during an exam (no IsCorrect).
/// </summary>
public class ExamQuestionDto
{
    public int QuestionId { get; set; }
    public string Body { get; set; } = string.Empty;
    public int Score { get; set; }
    public List<ExamChoiceDto> Choices { get; set; } = new();
}

/// <summary>
/// Represents a choice as seen by the student (no IsCorrect flag).
/// </summary>
public class ExamChoiceDto
{
    public int ChoiceId { get; set; }
    public string Body { get; set; } = string.Empty;
}
