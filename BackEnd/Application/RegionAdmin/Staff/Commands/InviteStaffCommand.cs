using LawyerProject.Application.Auth.Queries.GetUser;
using MediatR;

namespace LawyerProject.Application.RegionAdmin.Staff.Commands;
public class InviteStaffCommand : IRequest<AuthResult>
{
    public int RegionId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PhoneNumber { get; init; } = null!;
}
