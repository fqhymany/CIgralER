using LawyerProject.Application.Auth.Queries.GetUser;
using LawyerProject.Application.Common.Interfaces;

namespace LawyerProject.Application.Auth.Commands.Select_Region;

public record SelectRegionCommand : IRequest<AuthResult>
{
    public string UserId { get; init; } = null!;
    public int RegionId { get; init; }
}

public class SelectRegionCommandHandler : IRequestHandler<SelectRegionCommand, AuthResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public SelectRegionCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResult> Handle(SelectRegionCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.RegionsUsers)
            .ThenInclude(ru => ru.Region)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            return new AuthResult { Succeeded = false, Error = "Invalid user" };

        var selectedRegion = user.RegionsUsers.FirstOrDefault(ru => ru.RegionId == request.RegionId);
        if (selectedRegion == null)
            return new AuthResult { Succeeded = false, Error = "Invalid region" };

        var token = await _tokenService.GenerateTokenAsync(user, request.RegionId);
        var (refreshToken, refreshTokenExpiry) = await _tokenService.GenerateRefreshTokenAsync(user);

        return new AuthResult
        {
            Succeeded = true,
            Data = new AuthResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName!,
                AccessToken = token,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = refreshTokenExpiry,
                SelectedRegion = new RegionDto
                {
                    Id = selectedRegion.Region!.Id,
                    Name = selectedRegion.Region.Name,
                    DomainUrl = selectedRegion.Region.DomainUrl
                }
            }
        };
    }
}
