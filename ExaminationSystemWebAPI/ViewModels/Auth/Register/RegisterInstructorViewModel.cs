using ExaminationSystemWebAPI.Helpers;

namespace ExaminationSystemWebAPI.ViewModels.Auth.Register;

public class RegisterInstructorViewModel : RegisterViewModel
{
    public RegisterInstructorViewModel()
    {
        ClaimRole = CustomClaimTypes.ISINSTRUCTOR;
    }
}
