using LawyerProject.Application.Common.Exceptions;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.CaseTypes.Commands
{
    [Authorize]
    // Command to update an existing CaseType by its Id and new Name
    public record UpdateCaseTypeCommand : IRequest<int>
    {
        public int Id { get; init; }
        public required string Name { get; init; }
    }

    public class UpdateCaseTypeCommandHandler : IRequestHandler<UpdateCaseTypeCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;

        public UpdateCaseTypeCommandHandler(IApplicationDbContext context, IUser User)
        {
            _context = context;
            _user = User;
        }

        // Handle the UpdateCaseTypeCommand, update the CaseType, and save to the database
        public async Task<int> Handle(UpdateCaseTypeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var caseType = await _context.CaseTypes.FindAsync(request.Id);

                if (caseType == null)
                {
                    return 0;
                }
                if (caseType.RegionId == 0)
                    throw new ForbiddenAccessException($"عدم دسترسی به متغیر های سیستمی.");
                if (caseType.RegionId != _user.RegionId)
                    throw new ForbiddenAccessException($"عدم دسترسی به متغیر های سایر نواحی.");
                caseType.Name = request.Name;

                await _context.SaveChangesAsync(cancellationToken);

                return caseType.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"خطا: {ex.Message}", ex);
            }
        }
    }

}
