namespace ExaminationSystem.API.Models.Requests.StudentExams;

public class StartExamAttemptRequest
{
    /// <summary>
    /// The ID of the exam to start an attempt for.
    /// </summary>
    public int ExamId { get; set; }
}
