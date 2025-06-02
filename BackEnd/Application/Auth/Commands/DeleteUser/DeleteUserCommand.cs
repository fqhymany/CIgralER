using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Interfaces;

namespace LawyerProject.Application.Auth.Commands.DeleteUser;

[Authorize]
public record DeleteUserCommand : IRequest<bool>
{
    public string? Id { get; init; }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public DeleteUserCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // First ensure user exists
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
            throw new NotFoundException("User  not found.", "User  not found.");

        try
        {
            user.IsDelete = true;

            await _context.SaveChangesAsync(cancellationToken);


            return true;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while deleting user. Details: {ex.Message}", ex);
        }
    }
}
