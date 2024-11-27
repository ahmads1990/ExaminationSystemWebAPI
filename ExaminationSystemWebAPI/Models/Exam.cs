using ExaminationSystemWebAPI.Helpers.Enums;

namespace ExaminationSystemWebAPI.Models;

public class Exam : BaseModel
{
    public ExamType ExamType { get; set; }
    public int MaxDuration { get; set; }
    public int TotalGrade { get; set; }
    public int PassMark { get; set; }
    public bool IsPublished { get; set; }
    public DateTime DeadlineDate { get; set; }

    public string CourseID { get; set; } = string.Empty;
    public Course Course { get; set; } = default!;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
