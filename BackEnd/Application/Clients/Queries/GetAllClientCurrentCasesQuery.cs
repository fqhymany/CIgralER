using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Utils;
using LawyerProject.Application.Clients;
using LawyerProject.Domain.Entities;
using LawyerProject.Application.BasicInfo.CaseStatuses;
using LawyerProject.Application.BasicInfo.JudicialDeadline;
using LawyerProject.Application.BasicInfo.CasePredefinedSubjects;
using LawyerProject.Application.Cases.Queries.GetCases;
using LawyerProject.Application.UserRoles.Queries.GetUserNamesByRoleName;
using LawyerProject.Application.BasicInfo.ClientRolesInCase;

namespace LawyerProject.Application.Clients.Queries
{
    
    public record GetAllClientCasesQuery : IRequest<ClientCaseVm>
    {
        public required string ClientId { get; init; }
    }

    public class GetAllClientCasesQueryHandler : IRequestHandler<GetAllClientCasesQuery, ClientCaseVm>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetAllClientCasesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ClientCaseVm> Handle(GetAllClientCasesQuery request, CancellationToken cancellationToken)
        {
            // Get RoleId for 'Client'
            var clientRoleId = await UserRoleUtils.GetRoleIdByRoleNameAsync(_context, "Client", cancellationToken);

            // Find all case participants with the given ClientId
            var clientCases = await _context.CaseParticipants
                .Where(cp => cp.UserId == request.ClientId && cp.RoleId == clientRoleId && cp.Case.IsDeleted == false)
                .OrderBy(cp => cp.CaseId)
                .Select(cp => new ClientCaseDto
                {
                    Id = cp.Case.Id,
                    CaseNumber = cp.Case.CaseNumber,
                    Title = cp.Case.Title,
                    EndDate = cp.Case.EndDate.HasValue ? DateUtils.ToPersianDateTime(cp.Case.EndDate.Value) : "نامشخص",
                    CaseStatus = cp.Case.CaseStatusId.HasValue ? new CaseStatusDto
                    {
                        Id = cp.Case.CaseStatusId.Value,
                        Name = _context.CaseStatuss
                            .Where(cs => cs.Id == cp.Case.CaseStatusId.Value)
                            .Select(cs => cs.Name)
                            .FirstOrDefault() ?? "نامشخص"
                    } : null,
                    nearestDeadline = cp.Case.JudicialDeadlines
                        .Where(cp => cp.IsDeleted == false && cp.CaseId == cp.Case.Id && cp.EndDate.HasValue && cp.EndDate.Value >= DateTime.Now) // only non-passed deadlines
                        .OrderBy(cp => cp.EndDate)
                        .Select(cp => new JudicialDeadlineDto
                        {
                            Id = cp.Id,
                            CaseId = cp.CaseId,
                            Title = cp.Title,
                            DeadlineType = cp.DeadlineType,
                            StartDate = cp.StartDate.HasValue ? DateUtils.ToPersianDateTime(cp.StartDate.Value) : "نامشخص",
                            EndDate = cp.EndDate.HasValue ? DateUtils.ToPersianDateTime(cp.EndDate.Value) : "نامشخص"
                        })
                        .FirstOrDefault(),
                    SubjectType = cp.Case.SubjectType,
                    PredefinedSubject = cp.Case.PredefinedSubjectId.HasValue ? new PredefinedSubjectDto
                    {
                        Id = cp.Case.PredefinedSubjectId.Value,
                        Title = _context.PredefinedSubjects
                            .Where(ps => ps.Id == cp.Case.PredefinedSubjectId.Value)
                            .Select(ps => ps.Title)
                            .FirstOrDefault() ?? "نامشخص"
                    } : null,
                    ClientRoleInCase = cp.ClientRoleInCaseId.HasValue ? new ClientRoleInCaseDto
                    {
                        Id = cp.ClientRoleInCaseId.Value,
                        Title = _context.ClientRolesInCase
                            .Where(cr => cr.Id == cp.ClientRoleInCaseId.Value)
                            .Select(cr => cr.Title)
                            .FirstOrDefault() ?? "نامشخص"
                    } : null,
                    CaseDetails = cp.Case.DetailsStage
                        .Where(ds => ds.CaseId == cp.Case.Id)
                        .Select(ds => new CaseDetailsStageDto
                        {
                            JudgeId = ds.JudgeId,
                            HearingStageId = ds.HearingStageId,
                            ArchiveNumber = ds.ArchiveNumber,
                            Date = ds.Date.HasValue ? DateUtils.ToPersianDate(ds.Date.Value) : "نامشخص",
                        }).FirstOrDefault(),
                })
                .ToListAsync(cancellationToken);

            return new ClientCaseVm { ClientCases = clientCases };
        }
    }
}
