using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;

namespace LawyerProject.Application.BasicInfo.CourtSubTypes.Queries
{
    [Authorize]
    // The query to fetch all CourtSubTypes, requiring authorization to execute
    public record GetAllCourtSubTypesQuery : IRequest<List<CourtSubtypeDto>>;

    public class GetAllCourtSubTypesQueryHandler : IRequestHandler<GetAllCourtSubTypesQuery, List<CourtSubtypeDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUser _user;

        public GetAllCourtSubTypesQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
        {
            _context = context;
            _mapper = mapper;
            _user = user;
        }

        // Handle the query and return the list of CourtSubTypes
        public async Task<List<CourtSubtypeDto>> Handle(GetAllCourtSubTypesQuery request, CancellationToken cancellationToken)
        {
            var caseCourtSubTypes = await _context.CourtSubtypes
                .AsNoTracking()
                .Where(cs => cs.IsDeleted == false && _user.RegionId == cs.RegionId)
                .OrderBy(cs => cs.Id)
                .Select(cs => new CourtSubtypeDto
                {
                    Id = Convert.ToString(cs.Id),
                    Title = cs.Title,
                    CourtTypeId = cs.CourtTypeId,
                })
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<CourtSubtypeDto>>(caseCourtSubTypes);
        }
    }
}
