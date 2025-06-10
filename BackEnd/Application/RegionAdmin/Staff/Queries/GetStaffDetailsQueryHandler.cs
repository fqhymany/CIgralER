using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.RegionAdmin.Staff.Queries;
using MediatR;

namespace LawyerProject.Application.RegionAdmin.Staff.Queries;


public class GetStaffDetailsQueryHandler
    : IRequestHandler<GetStaffDetailsQuery, StaffDetailsDto?>
{
    private readonly IIdentityService _identity;
    private readonly IUser _user;
    public GetStaffDetailsQueryHandler(IIdentityService identity, IUser user)
    {
        _identity = identity;
        _user = user;
    }

    public async Task<StaffDetailsDto?> Handle(
        GetStaffDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _identity.GetUserByIdAsync(request.UserId);
        if (user == null) return null;

        var allRoles = await _identity.GetUserRolesAsync(user.Id);
        var inRegion = user.RegionsUsers
            .FirstOrDefault(r => r.RegionId == _user.RegionId);
        if (inRegion == null) return null;

        return new StaffDetailsDto
        {
            Id = user.Id,
            FullName = $"{user.FirstName} {user.LastName}"
        };
    }
}
