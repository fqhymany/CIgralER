using LawyerProject.Application.Auth.Queries.GetUser;
using LawyerProject.Application.Common.Interfaces;

namespace LawyerProject.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<AuthResult>
{
    public string refreshToken { get; init; } = null!;
    public string accessToken { get; init; } = null!;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IUser _currentUser;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        IUser currentUser)
    {
        _context = context;
        _tokenService = tokenService;
        _currentUser = currentUser;
    }

    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.Id;
        if (userId == null)
            return new AuthResult { Succeeded = false, Error = "Invalid refreshToken" };

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return new AuthResult { Succeeded = false, Error = "User not found" };

        var regionId = _currentUser.RegionId;
        
        var newToken = await _tokenService.GenerateTokenAsync(user, regionId);
        return new AuthResult
        {
            Succeeded = true,
            Data = new AuthResponseDto
            {
                AccessToken = newToken,
                UserId = user.Id,
                UserName = user.UserName!
            }
        };
    }
}
