using LawyerProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawyerProject.Infrastructure.Data.Configurations;

public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{

    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(d => d.RequesterUser)
            .WithMany(p => p.SupportTicketsAsRequester) // نیاز به افزودن ICollection<SupportTicket> SupportTicketsAsRequester در User.cs
            .HasForeignKey(d => d.RequesterUserId)
            .OnDelete(DeleteBehavior.NoAction); 

        builder.HasOne(d => d.RequesterGuest)
            .WithMany(p => p.SupportTickets)
            .HasForeignKey(d => d.RequesterGuestId)
            .OnDelete(DeleteBehavior.Cascade); // اگر مهمان حذف شد، تیکت هایش هم حذف شوند (یا SetNull)

        builder.HasOne(d => d.AssignedAgent)
            .WithMany(p => p.SupportTicketsAsAgent) // نیاز به افزودن ICollection<SupportTicket> SupportTicketsAsAgent در User.cs
            .HasForeignKey(d => d.AssignedAgentUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(d => d.ChatRoom)
            .WithOne()
            .HasForeignKey<SupportTicket>(d => d.ChatRoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.Status).HasConversion<int>(); // ذخیره enum به عنوان int
    }
}
