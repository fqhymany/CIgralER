using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using Microsoft.AspNetCore.Http;

namespace LawyerProject.Application.Auth.Queries.GetUser;

[Authorize]
public record GetUserProfileQuery(string UserId) : IRequest<UserProfileDto?>;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRegionService _regionService;
    private readonly IUser _currentUser;
    private readonly IIdentityService _identityService;
    public GetUserProfileQueryHandler(IApplicationDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, IRegionService regionService, IUser currentUser, IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _regionService = regionService;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<UserProfileDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var regionIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("RegionId");
        if (regionIdClaim == null)
            return null;

        var user = await _context.Users
            .AsNoTracking()
            .ProjectTo<UserProfileDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user != null)
        {
            user.RegionName = await _regionService.GetRegionNameById(_currentUser.RegionId,cancellationToken);
        }
        if (user != null)
        {
            user.Roles = (List<string>?)await _identityService.GetUserRolesAsync(user.Id);
        }
        return user;
    }
}
