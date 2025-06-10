namespace LawyerProject.Application.Common.Interfaces;

/// <summary>
/// Interface for generating temporary Credential.
/// </summary>
public interface ICredentialService
{
    /// <summary>
    /// Generates a temporary email address.
    /// </summary>
    /// <returns>Temporary email string.</returns>
    string GenerateTemporaryEmail();

    /// <summary>
    /// Generates a strong password.
    /// </summary>
    /// <param name="length">Desired password length.</param>
    /// <returns>Generated strong password.</returns>
    string GenerateStrongPassword(int length = 8);
}
