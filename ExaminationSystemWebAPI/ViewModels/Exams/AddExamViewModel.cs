using ExaminationSystemWebAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ExaminationSystemWebAPI.ViewModels.Exams;

public class AddExamViewModel
{
    [Range(0, 1)]
    public ExamType ExamType { get; set; }
    public int MaxDuration { get; set; }
    public int TotalGrade { get; set; }
    public int PassMark { get; set; }
    public bool IsPublished { get; set; }
    public DateTime DeadlineDate { get; set; }

    public string CourseID { get; set; } = string.Empty;
}
