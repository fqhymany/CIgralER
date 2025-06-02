using LawyerProject.Application.Common.Interfaces;

namespace LawyerProject.Application.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    private readonly IApplicationDbContext _context;

    public RegisterCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.FirstName)
            .NotEmpty()
            .MaximumLength(200);
        RuleFor(v => v.LastName)
            .NotEmpty()
            .MaximumLength(200);
    }

    public async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return !await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }
}
