using LawyerProject.Application.Cases.Queries.GetCases;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Domain.Enums;

namespace LawyerProject.Application.Clients.Commands;


public record UpdateClientCommand : IRequest<Unit>
{
    public string? Id { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string UserName { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string NationalCode { get; init; } = null!;
    public List<string> Roles { get; init; } = [];
}

public class UpdateClientCommandHandler : IRequestHandler<ClientDto, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateClientCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(ClientDto request, CancellationToken cancellationToken)
    {
        var client = await _context.Users
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (client == null)
        {
            throw new NotFoundException(nameof(client), request.Id?.ToString() ?? string.Empty);
        }

        // Update client properties
        _mapper.Map(request, client);

        // Update entity in database
        _context.Users.Update(client);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
