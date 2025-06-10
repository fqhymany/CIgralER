using MediatR;
using LawyerProject.Application.RolePermissions.DTOs;

namespace LawyerProject.Application.RolePermissions.Queries.GetPermissionsByRole;

public record GetPermissionsByRoleQuery(string RoleId) : IRequest<IEnumerable<RolePermissionDto>>;
