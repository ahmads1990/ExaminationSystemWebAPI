namespace ExaminationSystem.Application.DTOs.Questions;

public class ListQuestionsDto : BasePaginatedDto
{
    public int? ExamID { get; set; }
    public string? Body { get; set; }
}
