using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using MediatR;
using System;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.CaseStatuses.Commands
{
    [Authorize]
    // Command to soft delete an existing case status by its Id
    public record DeleteCaseStatusCommand : IRequest<bool>
    {
        public int Id { get; init; }
    }


    public class DeleteCaseStatusCommandHandler : IRequestHandler<DeleteCaseStatusCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;
        public DeleteCaseStatusCommandHandler(IApplicationDbContext context, IUser User)
        {
            _context = context;
            _user = User;
        }

        // Handle the DeleteCaseStatusCommand, delete the CaseStatus from the database
        public async Task<bool> Handle(DeleteCaseStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var isCaseStatusDeleted = await EntityDeletionHelper.SoftDeleteProtectedSystemRecordsAsync(_context, "CaseStatuss", request.Id, _user.RegionId, cancellationToken);

                if (!isCaseStatusDeleted)
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
