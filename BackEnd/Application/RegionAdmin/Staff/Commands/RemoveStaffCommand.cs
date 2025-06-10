using MediatR;
using LawyerProject.Application.Common.Models;

namespace LawyerProject.Application.RegionAdmin.Staff.Commands;
public class RemoveStaffCommand : IRequest<Result>
{
    public string UserId { get; set; } = null!;
    public int RegionId { get; set; }
}
