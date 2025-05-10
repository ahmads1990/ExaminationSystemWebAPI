namespace ExaminationSystem.Application.DTOs.Users;

public class UserClaim
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    //public int UserId { get; set; } = default!;
    //public AppUser User { get; set; } = default!;
    public UserClaim(string type, string value)
    {
        Type = type;
        Value = value;
    }
}   

