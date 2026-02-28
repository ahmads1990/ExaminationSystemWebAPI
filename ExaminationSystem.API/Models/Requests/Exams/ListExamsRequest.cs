namespace ExaminationSystem.API.Models.Requests.Exams;

public class ListExamsRequest : BasePaginatedRequest
{
    public string? Title { get; set; }
    public ExamType? ExamType { get; set; }
}
