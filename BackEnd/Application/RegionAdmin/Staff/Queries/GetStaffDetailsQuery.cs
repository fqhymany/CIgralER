using MediatR;
using LawyerProject.Application.RegionAdmin.Staff.Queries;

namespace LawyerProject.Application.RegionAdmin.Staff.Queries;
public record GetStaffDetailsQuery(string UserId) : IRequest<StaffDetailsDto?>
{
}
