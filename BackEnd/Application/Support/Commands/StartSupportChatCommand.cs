using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Domain.Entities;
using LawyerProject.Domain.Enums;

namespace LawyerProject.Application.Support.Commands;

public record StartSupportChatCommand(
    string? UserId,
    string? GuestSessionId,
    string? GuestName,
    string? GuestEmail,
    string? GuestPhone,
    string IpAddress,
    string? UserAgent,
    string InitialMessage
) : IRequest<StartSupportChatResult>;

public record StartSupportChatResult(
    int ChatRoomId,
    int TicketId,
    string? AssignedAgentId,
    string? AssignedAgentName
);

public class StartSupportChatCommandHandler : IRequestHandler<StartSupportChatCommand, StartSupportChatResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IAgentAssignmentService _agentAssignment;
    private readonly IChatHubService _chatHubService;

    public StartSupportChatCommandHandler(
        IApplicationDbContext context,
        IAgentAssignmentService agentAssignment,
        IChatHubService chatHubService)
    {
        _context = context;
        _agentAssignment = agentAssignment;
        _chatHubService = chatHubService;
    }

    public async Task<StartSupportChatResult> Handle(
        StartSupportChatCommand request,
        CancellationToken cancellationToken)
    {
        // 1. ایجاد یا یافتن Guest User
        GuestUser? guestUser = null;
        // Replace the existing GuestUser creation block with this:
        if (!string.IsNullOrEmpty(request.GuestSessionId))
        {
            // Try to find existing guest user, include any that might be inactive
            guestUser = await _context.GuestUsers
                .FirstOrDefaultAsync(g => g.SessionId == request.GuestSessionId, cancellationToken);

            if (guestUser == null)
            {
                // Check if this session ID exists but was deleted
                var existingInactive = await _context.GuestUsers
                    .IgnoreQueryFilters() // In case you have soft delete filters
                    .FirstOrDefaultAsync(g => g.SessionId == request.GuestSessionId, cancellationToken);

                if (existingInactive != null)
                {
                    // Update the existing record instead of creating new
                    existingInactive.Name = request.GuestName;
                    existingInactive.Email = request.GuestEmail;
                    existingInactive.Phone = request.GuestPhone;
                    existingInactive.IpAddress = request.IpAddress;
                    existingInactive.UserAgent = request.UserAgent;
                    existingInactive.LastActivityAt = DateTime.UtcNow;
                    existingInactive.IsActive = true;
                    guestUser = existingInactive;
                }
                else
                {
                    // Create new guest user only if no existing record found
                    guestUser = new GuestUser
                    {
                        SessionId = request.GuestSessionId,
                        Name = request.GuestName,
                        Email = request.GuestEmail,
                        Phone = request.GuestPhone,
                        IpAddress = request.IpAddress,
                        UserAgent = request.UserAgent,
                        LastActivityAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.GuestUsers.Add(guestUser);
                }
            }
            else
            {
                // Update existing active guest info
                guestUser.LastActivityAt = DateTime.UtcNow;
                guestUser.IsActive = true;
                if (!string.IsNullOrEmpty(request.GuestName))
                    guestUser.Name = request.GuestName;
                if (!string.IsNullOrEmpty(request.GuestEmail))
                    guestUser.Email = request.GuestEmail;
                if (!string.IsNullOrEmpty(request.GuestPhone))
                    guestUser.Phone = request.GuestPhone;
                guestUser.IpAddress = request.IpAddress;
                guestUser.UserAgent = request.UserAgent;
            }
        }
        // 2. پیدا کردن بهترین Agent
        var assignedAgent = await _agentAssignment.GetBestAvailableAgentAsync(cancellationToken);

        // 3. ایجاد Chat Room
        var chatRoom = new ChatRoom
        {
            Name = !string.IsNullOrEmpty(request.UserId)
                ? "Support Chat - User"
                : $"Support Chat - {request.GuestName ?? "Guest"}",
            Description = "Live support chat",
            IsGroup = false,
            ChatRoomType = ChatRoomType.Support,
            CreatedById = request.UserId ?? assignedAgent?.Id,
            GuestIdentifier = guestUser?.SessionId
        };
        _context.ChatRooms.Add(chatRoom);
        await _context.SaveChangesAsync(cancellationToken);

        // 4. اضافه کردن Members
        if (!string.IsNullOrEmpty(request.UserId))
        {
            _context.ChatRoomMembers.Add(new ChatRoomMember
            {
                UserId = request.UserId,
                ChatRoomId = chatRoom.Id,
                Role = ChatRole.Member
            });
        }

        if (assignedAgent != null)
        {
            _context.ChatRoomMembers.Add(new ChatRoomMember
            {
                UserId = assignedAgent.Id,
                ChatRoomId = chatRoom.Id,
                Role = ChatRole.Admin
            });
        }

        // 5. ایجاد Support Ticket
        var ticket = new SupportTicket
        {
            RequesterUserId = request.UserId,
            RequesterGuestId = guestUser?.Id,
            AssignedAgentUserId = assignedAgent?.Id,
            ChatRoomId = chatRoom.Id,
            Status = SupportTicketStatus.Open
        };
        _context.SupportTickets.Add(ticket);

        // 6. ارسال پیام اولیه
        var initialMessage = new ChatMessage
        {
            Content = request.InitialMessage,
            SenderId = request.UserId,
            ChatRoomId = chatRoom.Id,
            Type = MessageType.Text
        };
        _context.ChatMessages.Add(initialMessage);

        await _context.SaveChangesAsync(cancellationToken);

        // 7. Notify via SignalR
        if (assignedAgent != null)
        {
            await _chatHubService.NotifyAgentOfNewChat(assignedAgent.Id, chatRoom.Id);
        }

        return new StartSupportChatResult(
            chatRoom.Id,
            ticket.Id,
            assignedAgent?.Id,
            assignedAgent != null ? $"{assignedAgent.FirstName} {assignedAgent.LastName}" : null
        );
    }
}
