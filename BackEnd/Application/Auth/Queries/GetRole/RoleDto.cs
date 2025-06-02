using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.Auth.Queries.GetRole;

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
}
public class RoleMappingProfile : Profile
{
    public RoleMappingProfile()
    {
        CreateMap<Role, RoleDto>();
    }
}
