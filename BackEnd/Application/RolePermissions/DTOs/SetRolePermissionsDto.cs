namespace LawyerProject.Application.RolePermissions.DTOs;

public class SetRolePermissionsDto
{
    public string RoleId { get; set; } = string.Empty;
    public List<RoleSectionPermissionDto> Permissions { get; set; } = new();
}
