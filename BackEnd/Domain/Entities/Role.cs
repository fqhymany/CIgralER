using Microsoft.AspNetCore.Identity;

namespace LawyerProject.Domain.Entities;

public partial class Role : IdentityRole
{
    public string? Description { get; set; }

    public DateTime Created { get; set; } = DateTime.Now;

    public string? CreatedBy { get; set; }

    public DateTime LastModified { get; set; } = DateTime.Now;

    public string? LastModifiedBy { get; set; }
    public bool IsSystemRole { get; init; } = false;

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public virtual ICollection<UsersRole> UsersRoles { get; set; } = new List<UsersRole>();

    public virtual ICollection<CaseParticipant> CaseParticipants { get; set; } = new List<CaseParticipant>();
}
