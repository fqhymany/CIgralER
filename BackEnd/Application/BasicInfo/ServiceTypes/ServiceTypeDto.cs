using LawyerProject.Domain.Entities.CaseFinancials;

namespace LawyerProject.Application.BasicInfo.CaseTypes
{
    public class ServiceTypeDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ServiceType, ServiceTypeDto>();
            }
        }
    }
}
