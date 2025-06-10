using LawyerProject.Application.Clients.Commands;
using LawyerProject.Application.TodoLists.Queries.GetTodos;
using LawyerProject.Domain.Common;
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.Clients
{
    public class ClientDto :IHasRegion, IRequest<Unit>
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ClientRole { get; set; }
        public string? NationalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string>? Roles { get; set; } = [];
        public int RegionId { get; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<User, ClientDto>();

                // Map from Command to Entity (for updates)
                CreateMap<UpdateClientCommand, User>()
                    .ForAllMembers(opts => opts
                        .Condition((src, dest, srcMember) => srcMember != null));
            }
        }

    }
}
