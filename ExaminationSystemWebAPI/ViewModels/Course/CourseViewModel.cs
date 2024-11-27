namespace ExaminationSystemWebAPI.ViewModels.Course;

public class CourseViewModel
{    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CreditHours { get; set; }
    public string InstructorID { get; set; } = string.Empty;
}
