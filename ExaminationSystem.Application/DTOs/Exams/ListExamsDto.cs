namespace ExaminationSystem.Application.DTOs.Exams;

public class ListExamsDto : BasePaginatedDto
{
    public string? Title { get; set; }
    public ExamType? ExamType { get; set; }
}
