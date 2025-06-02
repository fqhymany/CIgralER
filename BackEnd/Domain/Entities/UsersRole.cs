using Microsoft.AspNetCore.Identity;

namespace LawyerProject.Domain.Entities;

public partial class UsersRole : IdentityUserRole<string>
{
    public int RegionId { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;

    public string? CreatedBy { get; set; }

    public DateTime LastModified { get; set; } = DateTime.Now;

    public string? LastModifiedBy { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual Region Region { get; set; } = null!;
}
