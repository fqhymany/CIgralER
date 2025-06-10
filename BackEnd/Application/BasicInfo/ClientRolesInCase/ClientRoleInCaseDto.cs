using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.ClientRolesInCase
{
    public class ClientRoleInCaseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CaseTypeId { get; set; } = 0;
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ClientRoleInCase, ClientRoleInCaseDto>();
            }
        }
    }
}
