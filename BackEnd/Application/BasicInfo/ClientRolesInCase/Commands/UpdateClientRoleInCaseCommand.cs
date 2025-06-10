using LawyerProject.Application.Common.Exceptions;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.ClientRolesInCase.Commands
{
    [Authorize]
    // Command to update an existing Client Role In Case by its Id and new Title and CaseType
    public record UpdateClientRoleInCaseCommand : IRequest<int>
    {
        public int Id { get; init; }
        public required string Title { get; init; }
        public int CaseTypeId { get; init; }

    }

    public class UpdateClientRoleInCaseCommandHandler : IRequestHandler<UpdateClientRoleInCaseCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;

        public UpdateClientRoleInCaseCommandHandler(IApplicationDbContext context, IUser User)
        {
            _context = context;
            _user = User;
        }

        // Handle the UpdateClientRoleInCaseCommand, update the Client Role In Case, and save to the database
        public async Task<int> Handle(UpdateClientRoleInCaseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var clientRoleInCase = await _context.ClientRolesInCase.FindAsync(request.Id);
                if (clientRoleInCase == null)
                {
                    return 0;
                }
                var caseTypeExists = _context.CaseTypes.Any(c => c.Id == request.CaseTypeId && (c.RegionId == 0 || c.RegionId == _user.RegionId));
                if (!caseTypeExists)
                {
                    return 0;

                }
                if (clientRoleInCase.RegionId == 0)
                    throw new ForbiddenAccessException($"عدم دسترسی به متغیر های سیستمی.");
                if (clientRoleInCase.RegionId != _user.RegionId)
                    throw new ForbiddenAccessException($"عدم دسترسی به متغیر های سایر نواحی.");
                clientRoleInCase.Title = request.Title;
                clientRoleInCase.CaseTypeId = request.CaseTypeId;

                await _context.SaveChangesAsync(cancellationToken);

                return clientRoleInCase.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"خطا: {ex.Message}", ex);
            }
        }
    }
}
