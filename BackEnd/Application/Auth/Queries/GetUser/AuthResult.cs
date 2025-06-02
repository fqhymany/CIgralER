namespace LawyerProject.Application.Auth.Queries.GetUser;

public class AuthResult
{
    public bool Succeeded { get; init; }
    public string? Error { get; init; }
    public AuthResponseDto? Data { get; init; }
}
