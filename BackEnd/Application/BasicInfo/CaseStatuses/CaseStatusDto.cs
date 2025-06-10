using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.CaseStatuses
{
    public class CaseStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<CaseStatus, CaseStatusDto>();
            }
        }
    }
}
