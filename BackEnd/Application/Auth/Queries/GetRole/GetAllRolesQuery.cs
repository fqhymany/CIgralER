using LawyerProject.Application.Common.Interfaces;

namespace LawyerProject.Application.Auth.Queries.GetRole;

public record GetAllRolesQuery : IRequest<IList<RoleDto>>;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, IList<RoleDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public GetAllRolesQueryHandler(IIdentityService identityService, IMapper mapper)
    {
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<IList<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _identityService.GetAllRolesAsync();
        return _mapper.Map<IList<RoleDto>>(roles);
    }
}
