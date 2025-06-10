using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.Common.Interfaces;

public interface IRolePermissionRepository
{
    Task<IEnumerable<RolePermission>> GetByRoleIdAsync(string roleId, CancellationToken cancellationToken);
    Task<RolePermission?> GetByRoleIdAndSectionAsync(string roleId, string section, CancellationToken cancellationToken);
    Task AddAsync(RolePermission rolePermission, CancellationToken cancellationToken);
    void Update(RolePermission rolePermission);
    void Remove(RolePermission rolePermission);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
