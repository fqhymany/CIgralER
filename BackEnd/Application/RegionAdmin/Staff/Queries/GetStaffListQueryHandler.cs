using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Cases.Queries.GetCases;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Results;
using LawyerProject.Application.RegionAdmin.Staff.Queries;
using LawyerProject.Domain.Entities;
using MediatR;
using Result = LawyerProject.Application.Common.Models.Result;

namespace LawyerProject.Application.RegionAdmin.Staff.Queries;



public class GetStaffListQueryHandler
    : IRequestHandler<GetStaffListQuery, StaffListVm>
{
    private readonly IIdentityService _identity;
    private readonly IUser _user;
    public GetStaffListQueryHandler(IIdentityService identity, IUser user)
    {
        _identity = identity;
        _user = user;
    }

    public async Task<StaffListVm> Handle(
        GetStaffListQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _identity.GetUsersInRolesAsync(["Lawyer", "Express"]);
        var userIds = users.Select(u => u.Id).ToList();
        var usersRoles = await _identity.GetUsersRolesAsync(userIds);

        return new StaffListVm
        {
            RegionId = _user.RegionId,
            Staff = users.Select(u => new StaffDto
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                PhoneNumber = u.PhoneNumber,
                Email = u.Email,
                Roles = usersRoles.TryGetValue(u.Id, out var roles) ? roles : new List<string>()
            }).ToList()
        };
    }
}
