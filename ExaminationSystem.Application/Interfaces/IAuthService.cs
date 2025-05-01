using ExaminationSystem.Application.DTOs.Auth;

namespace ExaminationSystem.Application.Interfaces;

public interface IAuthService
{
    Task RegisterInstructor(RegisterInstructorDto registerInstructorDto);
}

