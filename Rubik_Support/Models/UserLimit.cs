using System;

namespace Rubik_Support.Models
{
    public class UserLimit
    {
        public int Id { get; set; }
        public string Identifier { get; set; }
        public string IdentifierType { get; set; }
        public DateTime LastTicketDate { get; set; }
        public int TicketCount { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime? BlockedUntil { get; set; }
    }
}