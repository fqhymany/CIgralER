using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Models;

namespace LawyerProject.Application.Auth.Commands.ResetPassword;

public record ResetPasswordCommand : IRequest<Result>
{
    public string Mobile { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string NewPassword { get; init; } = null!;
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.Mobile, cancellationToken);

        if (user == null ||
            user.PasswordResetCode != request.Code ||
            user.PasswordResetCodeExpiry < DateTime.UtcNow)
            return Result.Failure(["Invalid or expired code"]);

        var result = await _identityService.ResetPasswordAsync(user, user.PasswordResetCode, request.NewPassword);

        if (result.Succeeded)
        {
            user.PasswordResetCode = null;
            user.PasswordResetCodeExpiry = null;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
