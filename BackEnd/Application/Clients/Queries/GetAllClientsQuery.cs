using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Cases.Queries.GetCases;
using LawyerProject.Domain.Entities;
using LawyerProject.Application.Clients;
using LawyerProject.Application.UserRoles.Queries.GetUserNamesByRoleName;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LawyerProject.Application.Clients.Queries.GetAllClients
{
    // The query to fetch all Clients, requiring authorization to execute
    [Authorize]
    public record GetAllClientsQuery : IRequest<List<ClientDto>>;

    public class GetAllClientsQueryHandler : IRequestHandler<GetAllClientsQuery, List<ClientDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUser _currentUser;

        public GetAllClientsQueryHandler(IApplicationDbContext context, IMapper mapper, IMediator mediator, IUser currentUser)
        {
            _context = context;
            _mapper = mapper;
            _mediator = mediator;
            _currentUser = currentUser;
        }

        // Handle the query and return the list of Clients
        public async Task<List<ClientDto>> Handle(GetAllClientsQuery request, CancellationToken cancellationToken)
        {
            // Get all users that have roles where issystemrole is not active
            var usersWithNonSystemRoles = await _context.UsersRoles
                .AsNoTracking()
                .Where(ur => !_context.Roles.Any(r => r.Id == ur.RoleId && r.IsSystemRole == true))
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var clientDtos = new List<ClientDto>();

            foreach (var userId in usersWithNonSystemRoles)
            {
                // Check if the user is deleted before proceeding
                var userInfo = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsDelete == false, cancellationToken);

                // Skip this user if they are deleted or don't exist
                if (userInfo == null) continue;

                // Check if user belongs to the current user's region
                var regionExists = await _context.RegionsUsers
                    .AsNoTracking()
                    .AnyAsync(ru => ru.UserId == userId && ru.RegionId == _currentUser.RegionId, cancellationToken);

                // Skip if user is not in the current region
                if (!regionExists) continue;

                var clientDto = new ClientDto
                {
                    Id = userId,
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    PhoneNumber = userInfo.PhoneNumber
                };

                var caseParticipant = await _context.CaseParticipants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);

                if (caseParticipant != null)
                {
                    var clientRoleInCase = await _context.ClientRolesInCase
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cr => cr.Id == caseParticipant.ClientRoleInCaseId, cancellationToken);

                    if (clientRoleInCase != null)
                    {
                        clientDto.ClientRole = clientRoleInCase.Title;
                    }
                }

                clientDtos.Add(clientDto);
            }

            return clientDtos;
        }
    }
}
