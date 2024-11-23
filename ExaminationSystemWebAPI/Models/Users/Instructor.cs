namespace ExaminationSystemWebAPI.Models.Users;

public class Instructor : BaseModel
{
    public string AppUserID { get; set; } = string.Empty;
    public AppUser AppUser { get; set; } = default!;
}
