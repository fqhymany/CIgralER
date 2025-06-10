using LawyerProject.Application.TodoLists.Queries.GetTodos;
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.Judges
{
    public class JudgeDto
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public int? RegionId { get; set; }
        //private class Mapping : Profile
        //{
        //    public Mapping()
        //    {
        //        CreateMap<Domain.Entities.Judge, JudgeDto>();
        //    }
        //}
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Domain.Entities.Judge, JudgeDto>()
                    .ForMember(dest => dest.FullName,
                             opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
            }
        }
    }
}
