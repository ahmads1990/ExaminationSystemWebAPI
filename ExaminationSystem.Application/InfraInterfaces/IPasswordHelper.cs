namespace ExaminationSystem.Application.InfraInterfaces;

public interface IPasswordHelper
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string password);
}