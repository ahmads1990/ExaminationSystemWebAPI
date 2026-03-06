namespace ExaminationSystem.Application.DTOs.Exams;

public class AssignQuestionsToExamDto
{
    public int ExamID { get; set; }
    public List<int> QuestionIDs { get; set; } = new List<int>();
}
