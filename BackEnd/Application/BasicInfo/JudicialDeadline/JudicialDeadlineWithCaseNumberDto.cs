

namespace LawyerProject.Application.BasicInfo.JudicialDeadline
{
    // New DTO to include CaseNumber
    public class JudicialDeadlineWithCaseNoDto
    {
        public int Id { get; set; }
        public required int CaseId { get; set; }
        public string? Title { get; set; }
        public string? DeadlineType { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? CaseNumber { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Domain.Entities.JudicialDeadline, JudicialDeadlineWithCaseNoDto>();
            }
        }
    }

    public class JudicialDeadlineWithCaseNoVm
    {
        public IReadOnlyCollection<JudicialDeadlineWithCaseNoDto> Deadlines { get; init; } = Array.Empty<JudicialDeadlineWithCaseNoDto>();
    }
}
