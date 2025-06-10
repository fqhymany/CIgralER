using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.UserRoles.Queries.GetUserNamesByRoleName
{
    public class User‌BasicDto
    {
        public required string  Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<User, User‌BasicDto>();
            }
        }
    }
    public class UserRoleInfoVm
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public IReadOnlyCollection<User‌BasicDto> Users { get; init; } = Array.Empty<User‌BasicDto>();
    }
}
