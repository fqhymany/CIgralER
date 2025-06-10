using System.Threading.Tasks;

namespace LawyerProject.Domain.Entities;

public class UserPreference : BaseAuditableEntity
{
    public int RegionId { get; set; }
    public virtual Region Region { get; set; } = null!;
    public required string UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public required string Key { get; set; }
    public virtual PreferenceKey PreferenceKey { get; set; } = null!;
    public string Value { get; set; } = string.Empty;
}
