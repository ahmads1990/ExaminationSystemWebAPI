namespace ExaminationSystemWebAPI.Models.Users;

public class Student : BaseModel
{
    public byte Grade { get; set; }
    public string AppUserID { get; set; } = string.Empty;
    public AppUser AppUser { get; set; } = default!;
}
