using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using MediatR;
using System;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.CasePredefinedSubjects.Commands
{
    [Authorize]
    // Command to soft delete an existing case predefined subject by its Id
    public record DeleteCasePredefinedSubjectCommand : IRequest<bool>
    {
        public int Id { get; init; }
    }

    public class DeleteCasePredefinedSubjectCommandHandler : IRequestHandler<DeleteCasePredefinedSubjectCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;

        public DeleteCasePredefinedSubjectCommandHandler(IApplicationDbContext context, IUser User)
        {
            _context = context;
            _user = User;
        }

        // Handle the DeleteCasePredefinedSubjectCommand, delete the CasePredefinedSubject from the database
        public async Task<bool> Handle(DeleteCasePredefinedSubjectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var isPredefinedSubjectDeleted = await EntityDeletionHelper.SoftDeleteProtectedSystemRecordsAsync(_context, "PredefinedSubjects", request.Id, _user.RegionId, cancellationToken);

                if (!isPredefinedSubjectDeleted)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while deleting the casePredefinedSubject. Details: {ex.Message}", ex);
            }
        }
    }
}
