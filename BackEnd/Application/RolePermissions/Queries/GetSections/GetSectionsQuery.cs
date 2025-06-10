using MediatR;

namespace LawyerProject.Application.RolePermissions.Queries.GetSections;

public record GetSectionsQuery : IRequest<string[]>;
