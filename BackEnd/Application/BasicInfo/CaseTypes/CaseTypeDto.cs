using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.CaseTypes
{
    public class CaseTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Domain.Entities.CaseType, CaseTypeDto>();
            }
        }
    }
}
