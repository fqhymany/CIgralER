using LawyerProject.Application.Common.Exceptions;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.CasePredefinedSubject.Commands
{
    [Authorize]
    // Command to update an existing Case Predefined Subject by its Id and new Title and CaseType
    public record UpdateCasePredefinedSubjectCommand : IRequest<int>
    {
        public int Id { get; init; }
        public required string Title { get; init; }
        public int CaseTypeId { get; init; }

    }

    public class UpdateCasePredefinedSubjectCommandHandler : IRequestHandler<UpdateCasePredefinedSubjectCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;
        public UpdateCasePredefinedSubjectCommandHandler(IApplicationDbContext context, IUser User)
        {
            _context = context;
            _user = User;
        }

        // Handle the UpdateCasePredefinedSubjectCommand, update the Case Predefined Subject, and save to the database
        public async Task<int> Handle(UpdateCasePredefinedSubjectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var casePredefinedSubject = await _context.PredefinedSubjects.FindAsync(request.Id);
                if (casePredefinedSubject == null)
                {
                    return 0;
                }
                var caseTypeExists = _context.CaseTypes.Any(c => c.Id == request.CaseTypeId && (c.RegionId == 0 || c.RegionId == _user.RegionId));
                if (!caseTypeExists)
                {
                    return 0;

                }
                if (casePredefinedSubject.RegionId == 0)
                    throw new ForbiddenAccessException($"عدم دسترسی به متغیر های سیستمی.");
                if (casePredefinedSubject.RegionId != _user.RegionId)
                    throw new ForbiddenAccessException($"عدم دسترسی به متغیر های سایر نواحی.");
                casePredefinedSubject.Title = request.Title;
                casePredefinedSubject.CaseTypeId = request.CaseTypeId;

                await _context.SaveChangesAsync(cancellationToken);

                return casePredefinedSubject.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"خطا: {ex.Message}", ex);
            }
        }
    }
}
