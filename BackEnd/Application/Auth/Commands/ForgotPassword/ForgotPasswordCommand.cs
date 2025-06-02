using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Models;

namespace LawyerProject.Application.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand : IRequest<Result>
{
    public string Mobile { get; init; } = null!;
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ISmsService _smsService;

    public ForgotPasswordCommandHandler(
        IApplicationDbContext context,
        ISmsService smsService)
    {
        _context = context;
        _smsService = smsService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.Mobile, cancellationToken);

        if (user == null)
            return Result.Success(); // Don't reveal if user exists

        // Generate 6-digit code
        var code = Random.Shared.Next(100000, 999999).ToString();

        user.PasswordResetCode = code;
        user.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(15); // کد 15 دقیقه اعتبار دارد

        await _context.SaveChangesAsync(cancellationToken);
        _smsService.SendPasswordResetSmsAsync(user.PhoneNumber!, code);

        return Result.Success();
    }
}
