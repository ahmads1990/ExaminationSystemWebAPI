using ExaminationSystemWebAPI.ViewModels.Auth;
using ExaminationSystemWebAPI.ViewModels.Auth.Register;

namespace ExaminationSystemWebAPI.Services.AuthService;

public interface IAuthService
{
    Task<AuthViewModel> RegisterUser(RegisterViewModel registerViewModel);
    Task<AuthViewModel> LoginUser(LoginViewModel loginViewModel);
    Task<string> AddClaim(AddClaimViewModel addClaimViewModel);
}
