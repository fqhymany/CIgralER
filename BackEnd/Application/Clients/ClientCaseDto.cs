using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawyerProject.Application.BasicInfo.CasePredefinedSubjects;
using LawyerProject.Application.BasicInfo.CaseStatuses;
using LawyerProject.Application.BasicInfo.ClientRolesInCase;
using LawyerProject.Application.BasicInfo.JudicialDeadline;
using LawyerProject.Application.Cases.Queries.GetCases;
using LawyerProject.Domain.Enums;

namespace LawyerProject.Application.Clients
{
    public class ClientCaseDto
    {
        public int Id { get; set; }
        public string? CaseNumber { get; init; }
        public string? Title { get; set; }
        public string? EndDate { get; set; }
        public CaseStatusDto? CaseStatus { get; set; } = new CaseStatusDto();
        public SubjectType? SubjectType { get; init; }
        public PredefinedSubjectDto? PredefinedSubject { get; set; } = new PredefinedSubjectDto();
        public ClientRoleInCaseDto? ClientRoleInCase { get; set; } = new ClientRoleInCaseDto();
        public JudicialDeadlineDto? nearestDeadline { get; set; }
        public CaseDetailsStageDto? CaseDetails { get; set; } = new CaseDetailsStageDto();

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Domain.Entities.Case, ClientCaseDto>();
            }
        }
    }
    public class ClientCaseVm
    {
        public IReadOnlyCollection<ClientCaseDto> ClientCases { get; init; } = Array.Empty<ClientCaseDto>();
    }
}
