using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using MediatR;
using System;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.CaseTypes.Commands
{
    [Authorize]
    // Command to soft delete an existing case type by its Id
    public record DeleteCaseTypeCommand : IRequest<bool>
    {
        public int Id { get; init; }
    }

    public class DeleteCaseTypeCommandHandler : IRequestHandler<DeleteCaseTypeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;

        public DeleteCaseTypeCommandHandler(IApplicationDbContext context, IUser User)
        {
            _context = context;
            _user = User;
        }

        // Handle the DeleteCaseTypeCommand, delete the CaseType from the database
        public async Task<bool> Handle(DeleteCaseTypeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var isCaseTypeDeleted = await EntityDeletionHelper.SoftDeleteProtectedSystemRecordsAsync(_context, "CaseTypes", request.Id, _user.RegionId, cancellationToken);

                if (!isCaseTypeDeleted)
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
