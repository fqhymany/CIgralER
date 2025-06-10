using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;

namespace LawyerProject.Application.BasicInfo.ClientRolesInCase.Queries
{
    [Authorize]
    // The query to fetch all Client Roles In Case, requiring authorization to execute
    public record GetAllClientRolesInCaseQuery : IRequest<List<ClientRoleInCaseDto>>;

    public class GetAllClientRolesInCaseQueryHandler : IRequestHandler<GetAllClientRolesInCaseQuery, List<ClientRoleInCaseDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUser _user;

        public GetAllClientRolesInCaseQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
        {
            _context = context;
            _mapper = mapper;
            _user = user;
        }

        // Handle the query and return the list of Client Roles In Case
        public async Task<List<ClientRoleInCaseDto>> Handle(GetAllClientRolesInCaseQuery request, CancellationToken cancellationToken)
        {
            var ClientRolesInCase = await _context.ClientRolesInCase
                .AsNoTracking()
                .Where(cr => cr.IsDeleted == false && _user.RegionId == cr.RegionId)
                .OrderBy(cr => cr.Id)
                .Select(cr => new ClientRoleInCaseDto
                {
                    Id = cr.Id,
                    Title = cr.Title,
                    CaseTypeId = cr.CaseTypeId ?? 0,
                })
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<ClientRoleInCaseDto>>(ClientRolesInCase);
        }
    }
}
