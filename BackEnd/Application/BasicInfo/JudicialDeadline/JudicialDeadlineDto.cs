

namespace LawyerProject.Application.BasicInfo.JudicialDeadline
{
    public class JudicialDeadlineDto
    {
        public int Id { get; set; }
        public required int CaseId { get; set; }
        public string? Title { get; set; }
        public string? DeadlineType { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Domain.Entities.JudicialDeadline, JudicialDeadlineDto>();
            }
        }
    }
    public class JudicialDeadlineVm
    {
        public IReadOnlyCollection<JudicialDeadlineDto> Deadlines { get; init; } = Array.Empty<JudicialDeadlineDto>();
    }
}

