using LawyerProject.Domain.Common;
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.Auth.Queries.GetUser;
public class UserProfileDto
{
    public string Id { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public string? RegionName { get; set; }
    public List<string>? Roles { get; set; }
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.FullName,
                           opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        }
    }

}
