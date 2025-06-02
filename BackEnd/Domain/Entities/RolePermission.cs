using Microsoft.AspNetCore.Identity;

namespace LawyerProject.Domain.Entities;

public partial class RolePermission : IdentityRoleClaim<string>,IHasRegion
{
    public string Section { get; set; } = null!;

    public bool CanView { get; set; }

    public bool CanCreate { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

    public DateTime Created { get; set; } = DateTime.Now;

    public string? CreatedBy { get; set; }

    public DateTime LastModified { get; set; } = DateTime.Now;

    public string? LastModifiedBy { get; set; }

    public virtual Role Role { get; set; } = null!;
    public int RegionId { get; set; }
}
