using System;
using System.Collections.Generic;

namespace Rubik_Support.Models
{
    public class SupportMessage
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Message { get; set; }
        public int? SenderId { get; set; }
        public SenderType SenderType { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? EditedDate { get; set; }
        public int? EditedBy { get; set; }

        // Navigation Properties
        public string SenderName { get; set; }
        public List<SupportAttachment> Attachments { get; set; }
    }
}