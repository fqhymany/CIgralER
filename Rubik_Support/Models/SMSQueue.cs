using System;

namespace Rubik_Support.Models
{
    public class SMSQueue
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string RecipientMobile { get; set; }
        public string Message { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? SentDate { get; set; }
        public bool IsSent { get; set; }
        public bool IsCancelled { get; set; }
        public int RetryCount { get; set; }
    }
}