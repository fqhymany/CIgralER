using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;

namespace LawyerProject.Application.UserRoles.Queries.GetUserNamesByRoleName;
[Authorize]
public record GetUserNamesByRoleNameCommand : IRequest<UserRoleInfoVm>
{
    public required string RoleName { get; init; }
}

public class GetUserNamesByRoleNameHandler : IRequestHandler<GetUserNamesByRoleNameCommand, UserRoleInfoVm>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;
    public GetUserNamesByRoleNameHandler(IApplicationDbContext context, IMapper mapper, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }

    public async Task<UserRoleInfoVm> Handle(GetUserNamesByRoleNameCommand request, CancellationToken cancellationToken)
    {
        var regionId = _user.RegionId;
        var normalizedRoleName = request.RoleName.ToUpper();
        var role = await _context.Roles.AsNoTracking().Where(r => r.NormalizedName == normalizedRoleName)
            .Select(r => new { r.Id, r.Name }).SingleOrDefaultAsync(cancellationToken);
        if (role == null)
        {
            return new UserRoleInfoVm();
        }
        var userIdsInRole = _context.UsersRoles.AsNoTracking()
            .Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId);
        var userIdsInRegion = _context.RegionsUsers.AsNoTracking()
            .Where(ru => ru.RegionId == regionId).Select(ru => ru.UserId);
        var users = await _context.Users.AsNoTracking().Where(u => userIdsInRole.Contains(u.Id)&& userIdsInRegion.Contains(u.Id))
            .ProjectTo<UserBasicDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
        return new UserRoleInfoVm
        {
            RoleId = role.Id,
            RoleName = role.Name ?? string.Empty,
            Users = users
        };
    }


}

