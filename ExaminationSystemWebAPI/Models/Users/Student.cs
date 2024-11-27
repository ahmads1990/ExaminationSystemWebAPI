namespace ExaminationSystemWebAPI.Models.Users;

public class Student : BaseModel
{
    public byte Grade { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
