using System;

namespace Rubik_Support.Models
{
    public class SupportAgentLog
    {
        public int Id { get; set; }
        public int AgentId { get; set; }
        public string ActionType { get; set; }
        public DateTime ActionDate { get; set; }
        public int? Duration { get; set; }
        public string IP { get; set; }
    }
}