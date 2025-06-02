namespace LawyerProject.Application.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(v => v.Mobile)
            .NotEmpty()
            .MaximumLength(100);
    }
}
