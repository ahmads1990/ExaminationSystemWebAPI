using ExaminationSystem.Application.DTOs.Auth;

namespace ExaminationSystem.Application.Interfaces;

public interface IAuthService
{
    Task<(RegisterResult result, string token)> RegisterInstructor(RegisterInstructorDto registerInstructorDto);
}

