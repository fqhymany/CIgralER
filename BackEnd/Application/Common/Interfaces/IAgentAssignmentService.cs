using LawyerProject.Domain.Entities;
using LawyerProject.Domain.Enums;

namespace LawyerProject.Application.Common.Interfaces;

public interface IAgentAssignmentService
{
    Task<User?> GetBestAvailableAgentAsync(CancellationToken cancellationToken = default);
    Task<int> GetAgentWorkloadAsync(string agentId, CancellationToken cancellationToken = default);
    Task UpdateAgentStatusAsync(string agentId, AgentStatus status, CancellationToken cancellationToken = default);
}
