using ExaminationSystem.Application.InfraInterfaces;
using Microsoft.AspNetCore.Identity;

namespace ExaminationSystem.Infrastructure.Services.Auth;

#pragma warning disable CS8625
public class PasswordHelper : IPasswordHelper
{
    private readonly IPasswordHasher<string> _passwordHasher;

    public PasswordHelper(IPasswordHasher<string> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null, password);
    }

    public bool VerifyPassword(string hashedPassword, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, password);
        return result == PasswordVerificationResult.Success;
    }
}

