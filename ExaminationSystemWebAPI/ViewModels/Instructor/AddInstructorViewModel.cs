using System.ComponentModel.DataAnnotations;

namespace ExaminationSystemWebAPI.ViewModels.Instructor;

public class AddInstructorViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}
