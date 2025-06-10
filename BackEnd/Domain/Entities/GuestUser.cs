namespace LawyerProject.Domain.Entities;

public class GuestUser : BaseAuditableEntity // یا BaseEntity اگر BaseAuditableEntity فیلدهای زیادی دارد که نیاز نیست
{
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Name { get; set; } 

    public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
}
