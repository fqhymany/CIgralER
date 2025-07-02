using System;
using System.Collections.Generic;

namespace Rubik_Support.Models
{
    public class SupportTicket
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; }
        public int? VisitorId { get; set; }
        public int? UserId { get; set; }
        public int? SupportUserId { get; set; }
        public string Subject { get; set; }
        public TicketStatus Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public bool IsActive { get; set; }
        public string ConnectionId { get; set; }

        // Navigation Properties
        public SupportVisitor Visitor { get; set; }
        public string UserFullName { get; set; }
        public string SupportFullName { get; set; }
        public List<SupportMessage> Messages { get; set; }
    }
}