namespace ExaminationSystem.API.Models.Requests.Courses;

public class UpdateCourseRequest
{
    public int ID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CreditHours { get; set; }
}
