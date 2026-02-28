namespace ExaminationSystem.Application.DTOs.Exams;

/// <summary>
/// Data transfer object for assigning or unassigning questions to/from an exam.
/// </summary>
public class AssignQuestionsDto
{
    public int ExamId { get; set; }
    public List<int> QuestionIds { get; set; } = new();
}
