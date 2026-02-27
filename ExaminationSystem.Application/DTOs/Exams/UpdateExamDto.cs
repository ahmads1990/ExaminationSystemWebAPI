namespace ExaminationSystem.Application.DTOs.Exams;

public class UpdateExamDto
{
    public int ID { get; set; }
    public ExamType ExamType { get; set; }
    public int MaxDurationInMinutes { get; set; }
    public int TotalGrade { get; set; }
    public decimal PassingScore { get; set; }
    public int MaxAttempts { get; set; }
    public bool ShuffleQuestions { get; set; }
    public bool ShowResultsImmediately { get; set; }
    public DateTime DeadlineDate { get; set; }
}
