using MediatR;
using LawyerProject.Application.RolePermissions.DTOs;

namespace LawyerProject.Application.RolePermissions.Commands.SetRolePermissions;

public record SetRolePermissionsCommand : IRequest<Unit>
{
    public string RoleId { get; init; } = string.Empty;
    public List<RoleSectionPermissionDto> Permissions { get; init; } = new();
}
