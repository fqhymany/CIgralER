using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.JudicialActions
{
    public class JudicialActionDto
    {
        public int Id { get; set; }
        public required int CaseId { get; set; }
        public string? Description { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<JudicialAction, JudicialActionDto>();
            }
        }
    }
    public class JudicialActionVm
    {
        public IReadOnlyCollection<JudicialActionDto> Actions { get; init; } = Array.Empty<JudicialActionDto>();
    }
}

