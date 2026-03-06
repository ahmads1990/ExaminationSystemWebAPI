using ExaminationSystem.Application.InfraInterfaces;
using Microsoft.AspNetCore.Identity;

namespace ExaminationSystem.Infrastructure.Services.Auth;

#pragma warning disable CS8625
public class PasswordHelper : IPasswordHelper
{
    #region Fields

    private readonly IPasswordHasher<string> _passwordHasher;

    #endregion

    #region Constructors

    public PasswordHelper(IPasswordHasher<string> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null, password);
    }

    /// <inheritdoc />
    public bool VerifyPassword(string hashedPassword, string password)
    {
        if (string.IsNullOrEmpty(hashedPassword))
            return false;

        var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, password);
        return result == PasswordVerificationResult.Success;
    }

    #endregion
}

