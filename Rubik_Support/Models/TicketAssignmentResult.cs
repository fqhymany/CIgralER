namespace Rubik_Support.Models
{
    public class TicketAssignmentResult
    {
        public bool Success { get; set; }
        public SupportAgent AssignedAgent { get; set; }
        public string Message { get; set; }
        public AssignmentFailureReason? FailureReason { get; set; }
    }
}