using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Utils;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.JudicialActions.Commands
{
    [Authorize]
    [RequiresCaseAccess]
    public record CreateJudicialActionCommand : IRequest<int>
    {
        public required int CaseId { get; init; }
        public string? Description { get; init; }
    }

    public class CreateJudicialActionCommandHandler : IRequestHandler<CreateJudicialActionCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateJudicialActionCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateJudicialActionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = new JudicialAction
                {
                    CaseId = request.CaseId,
                    Description = request.Description
                };
                _context.JudicialActions.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return entity.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while creating the JudicialAction. Details: {ex.Message}", ex);
            }
        }
    }
}
