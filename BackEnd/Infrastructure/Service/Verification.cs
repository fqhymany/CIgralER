using LawyerProject.Application.Common.Interfaces;

namespace LawyerProject.Infrastructure.Services;

public class Verification : IVerification
{
    public string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(1000000, 9999999).ToString();
    }
}
