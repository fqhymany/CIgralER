namespace LawyerProject.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(v => v.refreshToken)
            .NotEmpty();
        RuleFor(v => v.accessToken)
            .NotEmpty();
    }
}
