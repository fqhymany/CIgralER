namespace LawyerProject.Application.Clients.Commands;

public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(v => v.LastName)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(v => v.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(v => v.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20);
    }
}
