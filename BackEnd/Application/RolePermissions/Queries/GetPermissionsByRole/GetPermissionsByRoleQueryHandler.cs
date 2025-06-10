using MediatR;
using Microsoft.EntityFrameworkCore;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.RolePermissions.DTOs;

namespace LawyerProject.Application.RolePermissions.Queries.GetPermissionsByRole;

public class GetPermissionsByRoleQueryHandler : IRequestHandler<GetPermissionsByRoleQuery, IEnumerable<RolePermissionDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public GetPermissionsByRoleQueryHandler(IApplicationDbContext dbContext, IIdentityService identityService, IUser user)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _user = user;
    }

    public async Task<IEnumerable<RolePermissionDto>> Handle(GetPermissionsByRoleQuery request, CancellationToken cancellationToken)
    {
        string roleId = await _identityService.GetRoleIdByName(request.RoleId) ?? request.RoleId;

        var permissions = await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == roleId && rp.RegionId == _user.RegionId)
            .Select(rp => new RolePermissionDto
            {
                Section = rp.Section,
                CanView = rp.CanView,
                CanCreate = rp.CanCreate,
                CanEdit = rp.CanEdit,
                CanDelete = rp.CanDelete
            })
            .ToListAsync(cancellationToken);

        return permissions;
    }
}
