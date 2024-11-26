using System.ComponentModel.DataAnnotations;

namespace ExaminationSystemWebAPI.ViewModels.Student;

public class AddStudentViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    public int Grade { get; set; }
}
