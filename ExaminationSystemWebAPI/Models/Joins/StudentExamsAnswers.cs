using ExaminationSystemWebAPI.Models.Users;

namespace ExaminationSystemWebAPI.Models.Joins;

public class StudentExamsAnswers : BaseModel
{
    public string StudentID { get; set; } = string.Empty;
    public Student Student { get; set; } = default!;

    public string ExamID { get; set; } = string.Empty;
    public Exam Exam { get; set; } = default!;

    public string QuestionID { get; set; } = string.Empty;
    public Question Question { get; set; } = default!;   

    public string ChoiceID { get; set; } = string.Empty;
    public Choice Choice { get; set; } = default!;
}
