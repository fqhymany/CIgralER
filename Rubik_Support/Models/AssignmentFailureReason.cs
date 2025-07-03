namespace Rubik_Support.Models
{
    public enum AssignmentFailureReason
    {
        NoAgentsAvailable,
        AllAgentsBusy,
        TimeoutWaitingForResponse,
        AgentDeclined,
        MaxAttemptsReached
    }
}