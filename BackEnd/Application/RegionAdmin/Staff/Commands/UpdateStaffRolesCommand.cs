using LawyerProject.Application.Auth.Queries.GetUser;
using MediatR;

namespace LawyerProject.Application.RegionAdmin.Staff.Commands;
public class UpdateStaffRolesCommand : IRequest<AuthResult>
{
    public string UserId { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
    public int RegionId { get; set; }
}
