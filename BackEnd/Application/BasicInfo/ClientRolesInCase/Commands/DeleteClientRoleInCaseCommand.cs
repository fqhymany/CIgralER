using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using MediatR;
using System;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.ClientRolesInCase.Commands
{
    [Authorize]
    // Command to soft delete an existing client role in case by its Id
    public record DeleteClientRoleInCaseCommand : IRequest<bool>
    {
        public int Id { get; init; }
    }

    public class DeleteClientRoleInCaseCommandHandler : IRequestHandler<DeleteClientRoleInCaseCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;
        public DeleteClientRoleInCaseCommandHandler(IApplicationDbContext context, IUser User)
        {
            _context = context;
            _user = User;
        }

        // Handle the DeleteClientRoleInCaseCommand, delete the ClientRoleInCase from the database
        public async Task<bool> Handle(DeleteClientRoleInCaseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var isClientRoleInCaseDeleted = await EntityDeletionHelper.SoftDeleteProtectedSystemRecordsAsync(_context, "ClientRolesInCase", request.Id, _user.RegionId, cancellationToken);

                if (!isClientRoleInCaseDeleted)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"خطا: {ex.Message}", ex);
            }
        }
    }
}
