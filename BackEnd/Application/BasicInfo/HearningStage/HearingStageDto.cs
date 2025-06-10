using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.HearningStage
{
    public class HearingStageDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CaseTypeId { get; set; } = 0;
        public int CourtTypeId { get; set; } = 0;
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<HearingStage, HearingStageDto>()
                   .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CourtType.Title));
            }
        }
    }
}
