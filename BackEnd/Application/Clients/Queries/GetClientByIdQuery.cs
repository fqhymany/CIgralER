using LawyerProject.Application.Auth.Queries.GetUser;
using LawyerProject.Application.Common.Interfaces;

namespace LawyerProject.Application.Clients.Queries;

public record GetClientByIdQuery(Guid Id) : IRequest<ClientDto>;

// Handler
public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public GetClientByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
    }

    public async Task<ClientDto> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .ProjectTo<ClientDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(u => u.Id == request.Id.ToString(), cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(user), request.Id.ToString());
        }
        else
        {
            if (user.Id != null)
            {
                user.Roles = (List<string>?)await _identityService.GetUserRolesAsync(user.Id);
            }
        }

        return user;
    }
}
