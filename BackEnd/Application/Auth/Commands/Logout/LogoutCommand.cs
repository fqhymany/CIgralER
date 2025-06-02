using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Models;

namespace LawyerProject.Application.Auth.Commands.Logout;

public record LogoutCommand : IRequest<Result>
{
    public string UserId { get; init; } = null!;
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public LogoutCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _tokenService.RevokeTokenAsync(request.UserId);
        return Result.Success();
    }
}
