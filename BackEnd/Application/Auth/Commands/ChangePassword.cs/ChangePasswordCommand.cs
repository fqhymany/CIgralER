using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Models;
using LawyerProject.Application.Common.Security;

namespace LawyerProject.Application.Auth.Commands.ChangePassword.cs;

[Authorize]
public record ChangePasswordCommand : IRequest<Result>
{
    public string UserId { get; init; } = null!;
    public string CurrentPassword { get; init; } = null!;
    public string NewPassword { get; init; } = null!;
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public ChangePasswordCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            return Result.Failure(["User not found"]);

        var result = await _identityService.ChangePasswordAsync(
            request.UserId, request.CurrentPassword, request.NewPassword);

        return result;
    }
}
