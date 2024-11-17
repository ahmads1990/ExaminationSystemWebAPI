namespace ExaminationSystemWebAPI.Models;

public class BaseModel
{
    public string ID { get; set; } = string.Empty;
    public bool Deleted { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}