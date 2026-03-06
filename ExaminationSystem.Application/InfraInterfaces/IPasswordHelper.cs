namespace ExaminationSystem.Application.InfraInterfaces;

/// <summary>
/// Provides methods for password hashing and verification.
/// </summary>
public interface IPasswordHelper
{
    /// <summary>
    /// Hashes a password.
    /// </summary>
    /// <param name="password">The plain-text password.</param>
    /// <returns>The hashed password.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="hashedPassword">The stored password hash.</param>
    /// <param name="password">The plain-text password to verify.</param>
    /// <returns>True if the password matches the hash, otherwise false.</returns>
    bool VerifyPassword(string hashedPassword, string password);
}