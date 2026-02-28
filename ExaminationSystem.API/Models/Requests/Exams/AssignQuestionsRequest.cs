namespace ExaminationSystem.API.Models.Requests.Exams;

public class AssignQuestionsRequest
{
    public int ExamId { get; set; }
    public List<int> QuestionIds { get; set; } = new();
}
