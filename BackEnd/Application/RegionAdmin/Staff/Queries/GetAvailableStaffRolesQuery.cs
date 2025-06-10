using LawyerProject.Application.Auth.Queries.GetRole;
using MediatR;
using LawyerProject.Application.Common.Models;

namespace LawyerProject.Application.RegionAdmin.Staff.Queries;
public record GetAvailableStaffRolesQuery() : IRequest<List<RoleDto>>;
