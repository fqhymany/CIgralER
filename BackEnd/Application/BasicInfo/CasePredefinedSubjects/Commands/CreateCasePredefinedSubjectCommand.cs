using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.CasePredefinedSubject.Commands
{
    [Authorize]
    // Command to create a new Case Predefined Subject with the title provided
    public record CreateCasePredefinedSubjectCommand : IRequest<int>
    {
        public required string Title { get; init; }
        public int CaseTypeId { get; init; }
        public int RegionId { get; init; }
    }

    public class CreateCasePredefinedSubjectCommandHandler : IRequestHandler<CreateCasePredefinedSubjectCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;

        public CreateCasePredefinedSubjectCommandHandler(IApplicationDbContext context,IUser user)
        {
            _context = context;
            _user = user;
        }

        // Handle the CreateCasePredefinedSubjectCommand, create the new Case Predefined Subject, and save to the database
        public async Task<int> Handle(CreateCasePredefinedSubjectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    throw new ArgumentException("Title cannot be empty or null.");
                }
                if (string.IsNullOrWhiteSpace(request.CaseTypeId.ToString()))
                {
                    throw new ArgumentException("CaseTypeId cannot be empty or null.");
                }

                var caseTypeExists = _context.CaseTypes.Any(c => c.Id == request.CaseTypeId && (c.RegionId == 0 || c.RegionId == _user.RegionId));
                if (!caseTypeExists)
                {
                    throw new ApplicationException("The specified CaseTypeId does not exist.");
                }

                var entity = new PredefinedSubject
                {
                    Title = request.Title,
                    CaseTypeId = request.CaseTypeId,
                    RegionId = _user.RegionId
                };

                _context.PredefinedSubjects.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return entity.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while creating the case predefined subjects. Details: {ex.Message}", ex);
            }
        }
    }
}
