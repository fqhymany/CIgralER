using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.CourtType
{
    public class CourtTypeDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<LawyerProject.Domain.Entities.CourtType, CourtTypeDto>();
            }
        }
    }
}
