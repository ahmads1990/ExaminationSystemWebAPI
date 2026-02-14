namespace ExaminationSystem.API.Models.Requests.Questions;

public class ListQuestionsRequest : BasePaginatedRequest
{
    public int? ExamID { get; set; }
    public string? Body { get; set; }
}
