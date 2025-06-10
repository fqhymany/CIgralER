using LawyerProject.Domain.Entities.CaseFinancials;

namespace LawyerProject.Application.BasicInfo.ServiceSubjects
{
    public class ServiceSubjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ServiceTypeId { get; set; } = 0;
        public int ServiceUnitId { get; set; } = 0;
        public string Percentage { get; set; } = string.Empty;
        public string Cost { get; set; } = string.Empty;
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ServiceSubject, ServiceSubjectDto>();
            }
        }
    }
}
