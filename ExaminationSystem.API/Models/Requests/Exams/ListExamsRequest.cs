namespace ExaminationSystem.API.Models.Requests.Exams;

public class ListExamsRequest : BasePaginatedRequest
{
    public string? Title { get; set; }
    public ExamType? ExamType { get; set; }
    public ExamStatus? ExamStatus { get; set; }
    public int? CourseId { get; set; }
    public int? InstructorId { get; set; }
    public DateTime? DeadlineFrom { get; set; }
    public DateTime? DeadlineTo { get; set; }
}
