using ExaminationSystemWebAPI.Helpers;

namespace ExaminationSystemWebAPI.ViewModels.Auth.Register;

public class RegisterStudentViewModel : RegisterViewModel
{
    public int Grade { get; set; }
    public RegisterStudentViewModel()
    {
        ClaimRole = CustomClaimTypes.ISSTUDENT;
    }
}
