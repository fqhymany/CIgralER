namespace LawyerProject.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public Boolean IsDeleted { get; set; } = false;
    public DateTime Created { get; set; } = DateTime.Now;

    public string? CreatedBy { get; set; }

    public DateTime LastModified { get; set; } = DateTime.Now;

    public string? LastModifiedBy { get; set; }
}
