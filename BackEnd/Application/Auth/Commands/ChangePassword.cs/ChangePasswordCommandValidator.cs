namespace LawyerProject.Application.Auth.Commands.ChangePassword.cs;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty();

        RuleFor(v => v.CurrentPassword)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100);

        RuleFor(v => v.NewPassword)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100);
    }
}
