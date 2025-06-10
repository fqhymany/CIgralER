namespace LawyerProject.Application.RolePermissions.DTOs;

public class RolePermissionDto
{
    public string Section { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
