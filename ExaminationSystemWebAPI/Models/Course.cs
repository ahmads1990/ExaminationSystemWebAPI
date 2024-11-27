using ExaminationSystemWebAPI.Models.Users;

namespace ExaminationSystemWebAPI.Models;

public class Course : BaseModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CreditHours { get; set; }

    public string InstructorID { get; set; } = string.Empty;
    public Instructor Instructor { get; set; } = default!;

    public ICollection<Student> Students { get; set; } = new List<Student>();
}
