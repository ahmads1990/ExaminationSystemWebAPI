namespace ExaminationSystem.Application.DTOs.Exams;

public class UpdateExamDto
{
    public int ID { get; set; }
    public ExamType ExamType { get; set; }
    public int MaxDuration { get; set; }
    public int TotalGrade { get; set; }
    public int PassMark { get; set; }
    public bool IsPublished { get; set; }
    public DateTime DeadlineDate { get; set; }
}

