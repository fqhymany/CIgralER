using LawyerProject.Domain.Entities.CaseFinancials;

namespace LawyerProject.Application.BasicInfo.ServiceUnits
{
    public class ServiceUnitDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ServiceUnit, ServiceUnitDto>();
            }
        }
    }
}
