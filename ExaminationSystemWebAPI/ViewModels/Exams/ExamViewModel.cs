using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.ViewModels.Questions;

namespace ExaminationSystemWebAPI.ViewModels.Exams;

public class ExamViewModel
{
    public string ExamType { get; set; } = string.Empty;
    public int MaxDuration { get; set; }
    public int TotalGrade { get; set; }
    public int PassMark { get; set; }
    public bool IsPublished { get; set; }
    public DateTime DeadlineDate { get; set; }

    public ICollection<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
}
