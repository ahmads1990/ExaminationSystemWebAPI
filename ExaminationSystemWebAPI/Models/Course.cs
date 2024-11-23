namespace ExaminationSystemWebAPI.Models;

public class Course : BaseModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CreditHours { get; set; }
}
