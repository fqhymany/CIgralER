using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Auth.Queries.GetRole;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.RegionAdmin.Staff.Queries;
using MediatR;

namespace LawyerProject.Application.RegionAdmin.Staff.Queries;
public class GetAvailableStaffRolesQueryHandler
    : IRequestHandler<GetAvailableStaffRolesQuery, List<RoleDto>>
{
    private readonly IIdentityService _identity;
    public GetAvailableStaffRolesQueryHandler(IIdentityService identity)
        => _identity = identity;

    public async Task<List<RoleDto>> Handle(
        GetAvailableStaffRolesQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await _identity.GetAllRolesAsync();
        return roles
            .Select(r => new RoleDto { Id = r.Id, Name = r.Name })
            .ToList();
    }
}
