using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.CourtType.Queries.GetAllCourtTypes
{
    [Authorize]
    // The query to fetch all CourtTypes, requiring authorization to execute
    public record GetAllCourtTypesQuery : IRequest<List<CourtTypeDto>>;

    public class GetAllCourtTypesQueryHandler : IRequestHandler<GetAllCourtTypesQuery, List<CourtTypeDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUser _user;


        public GetAllCourtTypesQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
        {
            _context = context;
            _mapper = mapper;
            _user = user;
        }

        // Handle the query and return the list of CourtTypes
        public async Task<List<CourtTypeDto>> Handle(GetAllCourtTypesQuery request, CancellationToken cancellationToken)
        {
            var courtTypes = await _context.CourtTypes
                .AsNoTracking()
                .Where(ct => ct.IsDeleted == false && _user.RegionId == ct.RegionId)
                .OrderBy(ct => ct.Id)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<CourtTypeDto>>(courtTypes);
        }
    }
}
