namespace Rubik_Support.Models
{
    public class AgentWorkingHours
    {
        public string Day { get; set; } // Monday, Tuesday, etc.
        public string StartTime { get; set; } // "09:00"
        public string EndTime { get; set; } // "17:00"
        public bool IsWorkingDay { get; set; }
    }
}