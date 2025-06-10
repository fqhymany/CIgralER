
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.JudicialDecisions
{
    public class JudicialDecisionDto
    {
        public int Id { get; set; }
        public required int CaseId { get; set; }
        public string? Title { get; set; }
        public string? IssuedDate { get; set; }
        public string? DecisionType { get; set; }
        public string? DecisionNumber { get; set; }
        public string? DecisionOutcome { get; set; }
        public string? IssuingAuthority { get; set; }
        //public string? DecisionText { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<JudicialDecision, JudicialDecisionDto>();
            }
        }
    }
    public class JudicialDecisionVm
    {
        public IReadOnlyCollection<JudicialDecisionDto> Decisions { get; init; } = Array.Empty<JudicialDecisionDto>();
    }
}

