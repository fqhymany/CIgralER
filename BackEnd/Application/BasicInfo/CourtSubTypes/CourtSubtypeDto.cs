using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.CourtSubTypes
{
    public class CourtSubtypeDto
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public int CourtTypeId { get; set; } = 0;
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<CourtSubtype, CourtSubtypeDto>();
            }
        }
    }
}
