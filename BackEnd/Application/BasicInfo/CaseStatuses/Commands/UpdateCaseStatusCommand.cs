using LawyerProject.Application.Common.Exceptions;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.CasePredefinedSubject.Commands
{
    [Authorize]
    // Command to update an existing CaseStatus by its Id and new Name
    public record UpdateCaseStatusCommand : IRequest<int>
    {
        public int Id { get; init; }
        public required string Name { get; init; }
    }

    public class UpdateCaseStatusCommandHandler : IRequestHandler<UpdateCaseStatusCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;
        public UpdateCaseStatusCommandHandler(IApplicationDbContext context, IUser User)
        {
            _context = context;
            _user = User;
        }

        // Handle the UpdateCaseStatusCommand, update the CaseStatus, and save to the database
        public async Task<int> Handle(UpdateCaseStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var caseStatus = await _context.CaseStatuss.FindAsync(request.Id);
                if (caseStatus == null)
                    return 0;
                if(caseStatus.RegionId == 0)
                    throw new ForbiddenAccessException($"عدم دسترسی به متغیر های سیستمی.");
                if (caseStatus.RegionId != _user.RegionId)
                    throw new ForbiddenAccessException($"عدم دسترسی به متغیر های سایر نواحی.");
                caseStatus.Name = request.Name;
                await _context.SaveChangesAsync(cancellationToken);
                return caseStatus.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"خطا: {ex.Message}", ex);
            }
        }
    }
}
