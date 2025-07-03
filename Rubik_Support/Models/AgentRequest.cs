using System;

namespace Rubik_Support.Models
{
    public class AgentRequest
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int AgentId { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ResponseDate { get; set; }
        public bool? IsAccepted { get; set; }
        public DateTime TimeoutDate { get; set; }

        // Navigation
        public SupportAgent Agent { get; set; }
        public SupportTicket Ticket { get; set; }
    }
}