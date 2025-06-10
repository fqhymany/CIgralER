namespace LawyerProject.Application.RolePermissions.DTOs;

public class RoleSectionPermissionDto
{
    public string Section { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

