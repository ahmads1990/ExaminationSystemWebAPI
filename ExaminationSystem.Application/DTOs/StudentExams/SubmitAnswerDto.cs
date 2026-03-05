namespace ExaminationSystem.Application.DTOs.StudentExams;

/// <summary>
/// A single answer submitted by a student.
/// </summary>
public class SubmitAnswerDto
{
    public int QuestionId { get; set; }
    public int ChoiceId { get; set; }
}
